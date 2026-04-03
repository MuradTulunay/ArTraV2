using ArTraV2.Core.Formula;
using ArTraV2.Core.Indicators;

namespace ArTraV2.App.Dialogs;

public class FormulaEditorDialog : Form
{
    private readonly RichTextBox _codeEditor = new();
    private readonly ListView _errorList = new();
    private readonly TextBox _txtName = new();
    private readonly Button _btnCompile = new();
    private readonly Button _btnNew = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnAddToChart = new();
    private readonly Label _lblStatus = new();
    private readonly SplitContainer _splitMain = new();
    private readonly ListBox _lstMethods = new();
    private readonly TreeView _treeFormulas = new();
    private readonly SplitContainer _splitLeft = new();

    /// <summary>Selected indicator to add to chart (set when user clicks "Add to Chart")</summary>
    public string? SelectedFormulaName { get; private set; }

    // Category mapping from FML #region names
    private static readonly Dictionary<string, string> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bands"] = "Bands",
        ["Basic"] = "Basic",
        ["Index"] = "Index",
        ["Momentum"] = "Momentum",
        ["Oscillator"] = "Oscillators",
        ["PriceVol"] = "Price & Volume",
        ["Trend"] = "Trend",
        ["Volatility"] = "Volatility",
        ["Volume"] = "Volume",
        ["Trading"] = "Trading Systems",
        ["Scan"] = "Scan / Filter",
        ["TDSeq"] = "TD Sequential",
        ["Others"] = "Others",
    };

    public FormulaEditorDialog()
    {
        Text = "Formula Editor & Indicator Selector";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 34, 45);
        ForeColor = Color.FromArgb(209, 212, 220);
        MinimumSize = new Size(900, 600);

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
        _txtName.Width = 140;
        _txtName.BackColor = Color.FromArgb(42, 46, 57);
        _txtName.ForeColor = Color.White;
        _txtName.BorderStyle = BorderStyle.FixedSingle;

        _btnNew.Text = "New";
        _btnNew.Location = new Point(200, 5);
        _btnNew.Size = new Size(50, 26);
        _btnNew.FlatStyle = FlatStyle.Flat;
        _btnNew.BackColor = Color.FromArgb(42, 46, 57);
        _btnNew.ForeColor = Color.White;
        _btnNew.Click += (s, e) => NewFormula();

        _btnCompile.Text = "Compile (F5)";
        _btnCompile.Location = new Point(258, 5);
        _btnCompile.Size = new Size(95, 26);
        _btnCompile.FlatStyle = FlatStyle.Flat;
        _btnCompile.BackColor = Color.FromArgb(38, 166, 91);
        _btnCompile.ForeColor = Color.White;
        _btnCompile.Click += (s, e) => CompileCode();

        _btnSave.Text = "Save";
        _btnSave.Location = new Point(361, 5);
        _btnSave.Size = new Size(50, 26);
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.BackColor = Color.FromArgb(42, 46, 57);
        _btnSave.ForeColor = Color.White;
        _btnSave.Click += (s, e) => SaveFormula();

        _btnAddToChart.Text = "Add to Chart";
        _btnAddToChart.Location = new Point(420, 5);
        _btnAddToChart.Size = new Size(100, 26);
        _btnAddToChart.FlatStyle = FlatStyle.Flat;
        _btnAddToChart.BackColor = Color.FromArgb(41, 98, 255);
        _btnAddToChart.ForeColor = Color.White;
        _btnAddToChart.Click += (s, e) => AddSelectedToChart();

        _lblStatus.Text = "Ready";
        _lblStatus.Location = new Point(535, 9);
        _lblStatus.AutoSize = true;
        _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);

        toolbar.Controls.AddRange([lblName, _txtName, _btnNew, _btnCompile, _btnSave, _btnAddToChart, _lblStatus]);

        // Left panel: formula tree + method list
        _splitLeft.Orientation = Orientation.Horizontal;
        _splitLeft.Dock = DockStyle.Fill;
        _splitLeft.SplitterDistance = 400;
        _splitLeft.BackColor = Color.FromArgb(25, 29, 40);

        var treeLabel = new Label
        {
            Text = "  Indicators",
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.FromArgb(25, 29, 40),
            ForeColor = Color.FromArgb(150, 155, 165),
            Font = new Font("Segoe UI Semibold", 9f)
        };

        _treeFormulas.Dock = DockStyle.Fill;
        _treeFormulas.BackColor = Color.FromArgb(30, 34, 45);
        _treeFormulas.ForeColor = Color.FromArgb(209, 212, 220);
        _treeFormulas.BorderStyle = BorderStyle.None;
        _treeFormulas.Font = new Font("Segoe UI", 9f);
        _treeFormulas.AfterSelect += TreeFormulas_AfterSelect;
        _treeFormulas.NodeMouseDoubleClick += TreeFormulas_NodeDoubleClick;

        var methodLabel = new Label
        {
            Text = "  Built-in Functions",
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.FromArgb(25, 29, 40),
            ForeColor = Color.FromArgb(150, 155, 165),
            Font = new Font("Segoe UI Semibold", 9f)
        };

        _lstMethods.Dock = DockStyle.Fill;
        _lstMethods.BackColor = Color.FromArgb(30, 34, 45);
        _lstMethods.ForeColor = Color.FromArgb(209, 212, 220);
        _lstMethods.BorderStyle = BorderStyle.None;
        _lstMethods.Font = new Font("Segoe UI", 8.5f);
        _lstMethods.DoubleClick += LstMethods_DoubleClick;

        _splitLeft.Panel1.Controls.Add(_treeFormulas);
        _splitLeft.Panel1.Controls.Add(treeLabel);
        _splitLeft.Panel2.Controls.Add(_lstMethods);
        _splitLeft.Panel2.Controls.Add(methodLabel);

        // Main split: left panel | code editor + errors
        _splitMain.Dock = DockStyle.Fill;
        _splitMain.SplitterDistance = 260;
        _splitMain.BackColor = Color.FromArgb(30, 34, 45);

        var leftPanel = new Panel { Dock = DockStyle.Fill };
        leftPanel.Controls.Add(_splitLeft);
        _splitMain.Panel1.Controls.Add(leftPanel);

        // Right panel: code + errors
        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 500,
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
        _errorList.Columns.Add("", 22);
        _errorList.Columns.Add("Line", 50);
        _errorList.Columns.Add("Col", 40);
        _errorList.Columns.Add("Code", 65);
        _errorList.Columns.Add("Message", 600);
        _errorList.DoubleClick += ErrorList_DoubleClick;

        rightSplit.Panel2.Controls.Add(_errorList);
        rightSplit.Panel2.Controls.Add(errorLabel);

        _splitMain.Panel2.Controls.Add(rightSplit);

        Controls.Add(_splitMain);
        Controls.Add(toolbar);

        KeyPreview = true;
    }

    // --- Formula Tree (grouped by category from #region) ---

    private void PopulateFormulaTree()
    {
        _treeFormulas.BeginUpdate();
        _treeFormulas.Nodes.Clear();

        var types = FormulaBase.GetAllFormulaTypes();

        // Group by source file category (parsed from #region in .fml.cs files)
        var categorized = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in types)
        {
            var category = GetCategory(type);
            if (!categorized.ContainsKey(category))
                categorized[category] = [];
            categorized[category].Add(type);
        }

        foreach (var (category, list) in categorized.OrderBy(kv => kv.Key))
        {
            var displayName = CategoryMap.GetValueOrDefault(category, category);
            var groupNode = _treeFormulas.Nodes.Add(category, $"{displayName} ({list.Count})");
            groupNode.ForeColor = Color.FromArgb(120, 180, 255);

            foreach (var type in list.OrderBy(t => t.Name))
            {
                var node = groupNode.Nodes.Add(type.Name, type.Name);
                node.Tag = type;

                // Add description as tooltip
                try
                {
                    var instance = (FormulaBase?)Activator.CreateInstance(type);
                    if (instance != null && !string.IsNullOrEmpty(instance.LongName) && instance.LongName != type.Name)
                        node.Text = $"{type.Name} — {instance.LongName}";
                }
                catch { }
            }
        }

        _treeFormulas.ExpandAll();
        _treeFormulas.EndUpdate();
    }

    private static string GetCategory(Type type)
    {
        // Try to determine category from the source file's #region or assembly name
        var asm = type.Assembly;
        var asmName = asm.GetName().Name ?? "";

        // Built-in indicators compiled in ArTraV2.Core
        if (asmName.Contains("ArTraV2"))
        {
            // Use the DeclaringType's source file region hint
            // Parse from class attributes or naming convention
            var name = type.Name.ToUpperInvariant();

            // Common category detection by indicator name
            if (IsInCategory(name, ["RSI", "BIAS", "CCI", "ROC", "DBCD", "DPO", "FASTSTO", "SLOWSTO",
                "LWR", "SRDM", "VRSI", "WR", "SO", "MFI", "STOCHRSI", "RSIA", "FISHER", "B3612",
                "MOMENTUM", "MTM"]))
                return "Momentum";
            if (IsInCategory(name, ["BBI", "MACD", "VMACD", "DMA", "DMI", "ADX", "DDI", "CYS",
                "TRIX", "SAR", "PPO", "ICHIMOKU", "ZIG", "ZIGT", "ZIGTP", "EMA4", "MA4"]))
                return "Trend";
            if (IsInCategory(name, ["BOL", "BBIBOLL", "ENV", "CDP", "MIKE", "SR", "BBWIDTH", "HHLLV", "HHV"]))
                return "Bands";
            if (IsInCategory(name, ["AD", "MI", "MICD", "RC", "RCCD", "SRMI", "CMF", "ULT", "AROONOSC"]))
                return "Oscillator";
            if (IsInCategory(name, ["ASI", "OBV", "PVT", "SOBV", "WVAD"]))
                return "PriceVol";
            if (IsInCategory(name, ["ABI", "ADL", "ADR", "BT", "CHAIKIN", "MCO", "OBOS", "STIX"]))
                return "Index";
            if (IsInCategory(name, ["AMOUNT", "VOLMA", "VOSC", "VSTD", "PVO"]))
                return "Volume";
            if (IsInCategory(name, ["PSY", "VR", "ATR"]))
                return "Volatility";

            return "Others";
        }

        // Runtime compiled formulas
        return asmName.Replace("FML_Editor_", "").Replace("FML_", "Custom");
    }

    private static bool IsInCategory(string name, string[] indicators)
        => indicators.Any(i => name.Equals(i, StringComparison.OrdinalIgnoreCase));

    // --- Tree Events ---

    private void TreeFormulas_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is Type type)
        {
            try
            {
                var instance = (FormulaBase?)Activator.CreateInstance(type);
                var desc = instance?.Description ?? "";
                var parms = instance?.Params.Count > 0
                    ? string.Join(", ", instance.Params.Select(p => $"{p.Name}={p.DefaultValue}"))
                    : "none";
                _lblStatus.Text = $"{type.Name}  |  Params: {parms}";
                if (!string.IsNullOrEmpty(desc))
                    _lblStatus.Text += $"  |  {desc}";
            }
            catch
            {
                _lblStatus.Text = type.Name;
            }
            _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);
        }
    }

    private void TreeFormulas_NodeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is not Type type) return;

        // Load formula source into editor
        _txtName.Text = type.Name;
        _lblStatus.Text = $"Loaded: {type.Name}";
        _lblStatus.ForeColor = Color.FromArgb(38, 166, 91);

        // Try to find source in plugin files
        var pluginsDir = Path.Combine(
            Path.GetDirectoryName(typeof(FormulaBase).Assembly.Location) ?? "",
            "..", "..", "..", "..", "ArTraV2.Core", "Formula", "Plugins");

        // Also check common paths
        var searchPaths = new[]
        {
            pluginsDir,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"),
            @"C:\Users\Murad Tulunay\source\repos\ArTraV2\src\ArTraV2.Core\Formula\Plugins"
        };

        foreach (var dir in searchPaths)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir, "*.fml.cs"))
            {
                var content = File.ReadAllText(file);
                if (content.Contains($"class {type.Name}") || content.Contains($"class {type.Name}:"))
                {
                    _codeEditor.Text = content;
                    return;
                }
            }
        }

        // No source found — generate template
        _codeEditor.Text = FormulaCompiler.GetTemplate(type.Name);
        _lblStatus.Text = $"{type.Name} — source not found, template generated";
        _lblStatus.ForeColor = Color.FromArgb(255, 152, 0);
    }

    // --- Add to Chart ---

    private void AddSelectedToChart()
    {
        var node = _treeFormulas.SelectedNode;
        if (node?.Tag is Type type)
        {
            SelectedFormulaName = type.Name;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            _lblStatus.Text = "Select an indicator from the tree first";
            _lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
        }
    }

    // --- Compile ---

    private void NewFormula()
    {
        _txtName.Text = "NewIndicator";
        _codeEditor.Text = FormulaCompiler.GetTemplate("NewIndicator");
        _errorList.Items.Clear();
        _lblStatus.Text = "New formula created";
        _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);
    }

    private void CompileCode()
    {
        _errorList.Items.Clear();
        var result = FormulaCompiler.CompileAndRegister(_codeEditor.Text, $"FML_Editor_{_txtName.Text}");

        if (result.Success)
        {
            _lblStatus.Text = $"OK! Compiled in {result.CompileTimeMs:F0}ms";
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
            _lblStatus.Text = $"Saved: {Path.GetFileName(dlg.FileName)}";
            _lblStatus.ForeColor = Color.FromArgb(38, 166, 91);
        }
    }

    private void ErrorList_DoubleClick(object? sender, EventArgs e)
    {
        if (_errorList.SelectedItems.Count == 0) return;
        var err = (CompilerError)_errorList.SelectedItems[0].Tag!;

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

    // --- Method List ---

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
            "BARSSINCE(data)",
            "--- Statistical ---",
            "STD(data, N)",
            "AVEDEV(data, N)",
            "SLOPE(data, N)",
            "CORR(a, b, N)",
            "LR(data, N)",
            "--- Conditional ---",
            "IF(cond, trueVal, falseVal)",
            "CROSS(a, b)",
            "LONGCROSS(a, b, N)",
            "NOT(data)",
            "BETWEEN(data, low, high)",
            "EVERY(data, N)",
            "EXIST(data, N)",
            "FILTER(data, N)",
            "BACKSET(data, N)",
            "EXTEND(data)",
            "--- Math ---",
            "MAX(a, b) / MIN(a, b)",
            "ABS(data)",
            "SQRT(data) / SQR(data)",
            "POWER(data, N)",
            "LOG(data) / LN(data) / EXP(data)",
            "ROUND(data, decimals)",
            "--- Drawing ---",
            "DRAWICON(cond, price, icon)",
            "DRAWTEXT(cond, price, text)",
            "DRAWNUMBER(cond, price, val)",
            "STICKLINE(cond, p1, p2, w, h)",
            "--- Data Access ---",
            "OPEN / O",
            "HIGH / H",
            "LOW / L",
            "CLOSE / C",
            "VOLUME / V / VOL",
            "DATE / YEAR / MONTH / DAY",
        };

        foreach (var m in methods)
            _lstMethods.Items.Add(m);
    }

    private void LstMethods_DoubleClick(object? sender, EventArgs e)
    {
        if (_lstMethods.SelectedIndex < 0) return;
        var text = _lstMethods.SelectedItem?.ToString();
        if (text == null || text.StartsWith("---")) return;

        var insertText = text.Contains('(') ? text.Split('/')[0].Trim() : text.Split('/')[0].Trim();
        _codeEditor.SelectedText = insertText;
        _codeEditor.Focus();
    }
}
