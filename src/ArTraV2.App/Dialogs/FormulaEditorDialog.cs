using ArTraV2.Core.Formula;

namespace ArTraV2.App.Dialogs;

public class FormulaEditorDialog : Form
{
    private readonly RichTextBox _codeEditor = new();
    private readonly ListView _errorList = new();
    private readonly TextBox _txtName = new();
    private readonly Button _btnCompile = new();
    private readonly Button _btnNew = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnApply = new();
    private readonly Label _lblStatus = new();
    private readonly SplitContainer _splitMain = new();
    private readonly ListBox _lstMethods = new();
    private readonly TreeView _treeFormulas = new();
    private readonly SplitContainer _splitLeft = new();

    public FormulaEditorDialog()
    {
        Text = "Formula Editor";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 34, 45);
        ForeColor = Color.FromArgb(209, 212, 220);
        MinimumSize = new Size(800, 500);

        BuildUI();
        PopulateMethodList();
        PopulateFormulaTree();
    }

    private void BuildUI()
    {
        // Top toolbar
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.FromArgb(25, 29, 40),
            Padding = new Padding(4)
        };

        var lblName = new Label { Text = "Name:", Location = new Point(6, 9), AutoSize = true, ForeColor = Color.FromArgb(150, 155, 165) };
        _txtName.Text = "MyIndicator";
        _txtName.Location = new Point(50, 6);
        _txtName.Width = 150;
        _txtName.BackColor = Color.FromArgb(42, 46, 57);
        _txtName.ForeColor = Color.White;
        _txtName.BorderStyle = BorderStyle.FixedSingle;

        _btnNew.Text = "New";
        _btnNew.Location = new Point(210, 5);
        _btnNew.Size = new Size(50, 26);
        _btnNew.FlatStyle = FlatStyle.Flat;
        _btnNew.BackColor = Color.FromArgb(42, 46, 57);
        _btnNew.ForeColor = Color.White;
        _btnNew.Click += (s, e) => NewFormula();

        _btnCompile.Text = "Compile (F5)";
        _btnCompile.Location = new Point(270, 5);
        _btnCompile.Size = new Size(90, 26);
        _btnCompile.FlatStyle = FlatStyle.Flat;
        _btnCompile.BackColor = Color.FromArgb(38, 166, 91);
        _btnCompile.ForeColor = Color.White;
        _btnCompile.Click += (s, e) => CompileCode();

        _btnSave.Text = "Save";
        _btnSave.Location = new Point(370, 5);
        _btnSave.Size = new Size(50, 26);
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.BackColor = Color.FromArgb(42, 46, 57);
        _btnSave.ForeColor = Color.White;
        _btnSave.Click += (s, e) => SaveFormula();

        _btnApply.Text = "Apply & Close";
        _btnApply.Location = new Point(430, 5);
        _btnApply.Size = new Size(100, 26);
        _btnApply.FlatStyle = FlatStyle.Flat;
        _btnApply.BackColor = Color.FromArgb(41, 98, 255);
        _btnApply.ForeColor = Color.White;
        _btnApply.Click += (s, e) => ApplyAndClose();

        _lblStatus.Text = "Ready";
        _lblStatus.Location = new Point(550, 9);
        _lblStatus.AutoSize = true;
        _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);

        toolbar.Controls.AddRange([lblName, _txtName, _btnNew, _btnCompile, _btnSave, _btnApply, _lblStatus]);

        // Left panel: formula tree + method list
        _splitLeft.Orientation = Orientation.Horizontal;
        _splitLeft.Dock = DockStyle.Fill;
        _splitLeft.SplitterDistance = 350;
        _splitLeft.BackColor = Color.FromArgb(25, 29, 40);

        _treeFormulas.Dock = DockStyle.Fill;
        _treeFormulas.BackColor = Color.FromArgb(30, 34, 45);
        _treeFormulas.ForeColor = Color.FromArgb(209, 212, 220);
        _treeFormulas.BorderStyle = BorderStyle.None;
        _treeFormulas.AfterSelect += TreeFormulas_AfterSelect;
        _treeFormulas.DoubleClick += TreeFormulas_DoubleClick;

        _lstMethods.Dock = DockStyle.Fill;
        _lstMethods.BackColor = Color.FromArgb(30, 34, 45);
        _lstMethods.ForeColor = Color.FromArgb(209, 212, 220);
        _lstMethods.BorderStyle = BorderStyle.None;
        _lstMethods.DoubleClick += LstMethods_DoubleClick;

        _splitLeft.Panel1.Controls.Add(_treeFormulas);
        _splitLeft.Panel2.Controls.Add(_lstMethods);

        // Main split: left panel | code editor + errors
        _splitMain.Dock = DockStyle.Fill;
        _splitMain.SplitterDistance = 220;
        _splitMain.BackColor = Color.FromArgb(30, 34, 45);

        var leftPanel = new Panel { Dock = DockStyle.Fill };
        leftPanel.Controls.Add(_splitLeft);
        _splitMain.Panel1.Controls.Add(leftPanel);

        // Right panel: code + errors
        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 450,
            BackColor = Color.FromArgb(30, 34, 45)
        };

        // Code editor
        _codeEditor.Dock = DockStyle.Fill;
        _codeEditor.Font = new Font("Cascadia Code", 11f);
        _codeEditor.BackColor = Color.FromArgb(19, 23, 34);
        _codeEditor.ForeColor = Color.FromArgb(209, 212, 220);
        _codeEditor.BorderStyle = BorderStyle.None;
        _codeEditor.AcceptsTab = true;
        _codeEditor.WordWrap = false;
        _codeEditor.ScrollBars = RichTextBoxScrollBars.Both;
        _codeEditor.Text = FormulaCompiler.GetTemplate("MyIndicator");
        _codeEditor.KeyDown += CodeEditor_KeyDown;

        rightSplit.Panel1.Controls.Add(_codeEditor);

        // Error list
        var errorLabel = new Label
        {
            Text = "  Errors & Warnings",
            Dock = DockStyle.Top,
            Height = 20,
            BackColor = Color.FromArgb(25, 29, 40),
            ForeColor = Color.FromArgb(150, 155, 165),
            Font = new Font("Segoe UI", 8.5f)
        };

        _errorList.Dock = DockStyle.Fill;
        _errorList.View = View.Details;
        _errorList.FullRowSelect = true;
        _errorList.GridLines = true;
        _errorList.BackColor = Color.FromArgb(25, 29, 40);
        _errorList.ForeColor = Color.FromArgb(209, 212, 220);
        _errorList.BorderStyle = BorderStyle.None;
        _errorList.Font = new Font("Segoe UI", 8.5f);
        _errorList.Columns.Add("", 20);
        _errorList.Columns.Add("Line", 50);
        _errorList.Columns.Add("Col", 40);
        _errorList.Columns.Add("Code", 60);
        _errorList.Columns.Add("Message", 600);
        _errorList.DoubleClick += ErrorList_DoubleClick;

        rightSplit.Panel2.Controls.Add(_errorList);
        rightSplit.Panel2.Controls.Add(errorLabel);

        _splitMain.Panel2.Controls.Add(rightSplit);

        Controls.Add(_splitMain);
        Controls.Add(toolbar);

        // Keyboard shortcuts
        KeyPreview = true;
    }

    private void NewFormula()
    {
        _txtName.Text = "NewIndicator";
        _codeEditor.Text = FormulaCompiler.GetTemplate("NewIndicator");
        _errorList.Items.Clear();
        _lblStatus.Text = "New formula created";
    }

    private void CompileCode()
    {
        _errorList.Items.Clear();
        var result = FormulaCompiler.CompileAndRegister(_codeEditor.Text, $"FML_Editor_{_txtName.Text}");

        if (result.Success)
        {
            _lblStatus.Text = $"OK! - {result.CompileTimeMs:F0}ms";
            _lblStatus.ForeColor = Color.FromArgb(38, 166, 91);
            PopulateFormulaTree();
        }
        else
        {
            _lblStatus.Text = $"{result.Errors.Count(e => !e.IsWarning)} error(s)";
            _lblStatus.ForeColor = Color.FromArgb(231, 76, 60);

            foreach (var err in result.Errors)
            {
                var item = new ListViewItem(err.IsWarning ? "W" : "E");
                item.SubItems.Add(err.Line.ToString());
                item.SubItems.Add(err.Column.ToString());
                item.SubItems.Add(err.Id);
                item.SubItems.Add(err.Message);
                item.ForeColor = err.IsWarning ? Color.FromArgb(255, 152, 0) : Color.FromArgb(231, 76, 60);
                item.Tag = err;
                _errorList.Items.Add(item);
            }
        }
    }

    private void SaveFormula()
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "C# Formula|*.fml.cs|All Files|*.*",
            FileName = $"{_txtName.Text}.fml.cs"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, _codeEditor.Text);
            _lblStatus.Text = $"Saved: {dlg.FileName}";
            _lblStatus.ForeColor = Color.FromArgb(38, 166, 91);
        }
    }

    private void ApplyAndClose()
    {
        CompileCode();
        if (_errorList.Items.Cast<ListViewItem>().Any(i => ((CompilerError)i.Tag!).IsWarning == false))
            return; // Don't close if there are errors
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ErrorList_DoubleClick(object? sender, EventArgs e)
    {
        if (_errorList.SelectedItems.Count == 0) return;
        var err = (CompilerError)_errorList.SelectedItems[0].Tag!;

        // Navigate to error line in code editor
        if (err.Line > 0 && err.Line <= _codeEditor.Lines.Length)
        {
            var charIndex = _codeEditor.GetFirstCharIndexFromLine(err.Line - 1);
            if (charIndex >= 0)
            {
                _codeEditor.Select(charIndex + Math.Max(0, err.Column - 1), 0);
                _codeEditor.ScrollToCaret();
                _codeEditor.Focus();
            }
        }
    }

    private void CodeEditor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            CompileCode();
            e.Handled = true;
        }
    }

    private void PopulateMethodList()
    {
        var methods = new[]
        {
            "--- Moving Averages ---",
            "MA(data, period)",
            "EMA(data, period)",
            "SMA(data, N, M)",
            "DMA(data, alpha)",
            "--- Reference ---",
            "REF(data, N)",
            "HHV(data, N)",
            "LLV(data, N)",
            "HHVBARS(data, N)",
            "LLVBARS(data, N)",
            "--- Aggregate ---",
            "SUM(data, N)",
            "COUNT(data, N)",
            "BARSCOUNT(data)",
            "BARSLAST(data)",
            "--- Statistical ---",
            "STD(data, N)",
            "AVEDEV(data, N)",
            "SLOPE(data, N)",
            "--- Conditional ---",
            "IF(cond, trueVal, falseVal)",
            "CROSS(a, b)",
            "NOT(data)",
            "BETWEEN(data, low, high)",
            "EVERY(data, N)",
            "EXIST(data, N)",
            "FILTER(data, N)",
            "--- Math ---",
            "MAX(a, b)",
            "MIN(a, b)",
            "ABS(data)",
            "SQRT(data)",
            "POWER(data, N)",
            "LOG(data)",
            "EXP(data)",
            "--- Data Access ---",
            "OPEN / O",
            "HIGH / H",
            "LOW / L",
            "CLOSE / C",
            "VOLUME / V / VOL",
            "DATE",
        };

        foreach (var m in methods)
            _lstMethods.Items.Add(m);
    }

    private void LstMethods_DoubleClick(object? sender, EventArgs e)
    {
        if (_lstMethods.SelectedIndex < 0) return;
        var text = _lstMethods.SelectedItem?.ToString();
        if (text == null || text.StartsWith("---")) return;

        // Insert at cursor position
        var insertText = text.Contains('(') ? text : text.Split('/')[0].Trim();
        _codeEditor.SelectedText = insertText;
        _codeEditor.Focus();
    }

    private void PopulateFormulaTree()
    {
        _treeFormulas.Nodes.Clear();
        var root = _treeFormulas.Nodes.Add("Formulas");

        var types = FormulaBase.GetAllFormulaTypes();
        var groups = types.GroupBy(t => t.Namespace ?? "FML").OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var groupNode = root.Nodes.Add(group.Key);
            foreach (var type in group.OrderBy(t => t.Name))
            {
                var node = groupNode.Nodes.Add(type.Name);
                node.Tag = type;
            }
        }

        root.Expand();
        foreach (TreeNode node in root.Nodes)
            node.Expand();
    }

    private void TreeFormulas_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is Type type)
        {
            _lblStatus.Text = type.Name;
            _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);
        }
    }

    private void TreeFormulas_DoubleClick(object? sender, EventArgs e)
    {
        var node = _treeFormulas.SelectedNode;
        if (node?.Tag is not Type type) return;

        // Load formula source if available, otherwise show template
        try
        {
            var instance = (FormulaBase)Activator.CreateInstance(type)!;
            _txtName.Text = type.Name;

            // Try to find source file
            var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var sourceFile = Directory.Exists(pluginsDir)
                ? Directory.GetFiles(pluginsDir, "*.fml.cs").FirstOrDefault(f => File.ReadAllText(f).Contains($"class {type.Name}"))
                : null;

            if (sourceFile != null)
            {
                _codeEditor.Text = File.ReadAllText(sourceFile);
            }
            else
            {
                _lblStatus.Text = $"Source not available for {type.Name} — showing template";
                _codeEditor.Text = FormulaCompiler.GetTemplate(type.Name);
            }
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error: {ex.Message}";
        }
    }
}
