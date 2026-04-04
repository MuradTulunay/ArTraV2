using ArTraV2.Core.Indicators;

namespace ArTraV2.App.Controls;

public class ActiveIndicatorsPanel : Panel
{
    private readonly FlowLayoutPanel _flow = new();

    /// <summary>Fired when indicators are changed (added/removed/params edited)</summary>
    public event Action? Changed;

    /// <summary>Direct reference to chart's indicator list</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<IIndicator>? Indicators { get; set; }

    public ActiveIndicatorsPanel()
    {
        Dock = DockStyle.Right;
        Width = 230;
        BackColor = Color.FromArgb(25, 29, 40);
        Visible = false;

        var header = new Label
        {
            Text = "  Active Indicators",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(150, 155, 165),
            BackColor = Color.FromArgb(30, 34, 45),
            Font = new Font("Segoe UI Semibold", 9f),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _flow.Dock = DockStyle.Fill;
        _flow.AutoScroll = true;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.BackColor = Color.FromArgb(25, 29, 40);
        _flow.Padding = new Padding(4);

        Controls.Add(_flow);
        Controls.Add(header);
    }

    public void RefreshUI()
    {
        _flow.SuspendLayout();
        _flow.Controls.Clear();

        if (Indicators == null || Indicators.Count == 0)
        {
            Visible = false;
            _flow.ResumeLayout();
            return;
        }

        Visible = true;

        foreach (var ind in Indicators.ToList()) // ToList to avoid modification during iteration
        {
            var row = CreateIndicatorRow(ind);
            _flow.Controls.Add(row);
        }

        _flow.ResumeLayout();
    }

    private Panel CreateIndicatorRow(IIndicator indicator)
    {
        var paramCount = indicator.Parameters.Length;
        var rowHeight = Math.Max(32, 28 + paramCount * 22);

        var row = new Panel
        {
            Width = _flow.ClientSize.Width - 10,
            Height = rowHeight,
            BackColor = Color.FromArgb(30, 34, 45),
            Margin = new Padding(0, 2, 0, 2)
        };

        // Name label
        var lblName = new Label
        {
            Text = indicator.ShortName,
            Location = new Point(6, 4),
            Size = new Size(row.Width - 36, 18),
            ForeColor = Color.FromArgb(41, 98, 255),
            Font = new Font("Segoe UI Semibold", 9f)
        };
        row.Controls.Add(lblName);

        // Overlay badge
        if (indicator.IsOverlay)
        {
            var badge = new Label
            {
                Text = "OVL",
                Location = new Point(row.Width - 60, 6),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 150, 155, 165),
                Font = new Font("Segoe UI", 7f)
            };
            row.Controls.Add(badge);
        }

        // Remove button
        var btnRemove = new Button
        {
            Text = "\u2715",
            Size = new Size(24, 24),
            Location = new Point(row.Width - 30, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(231, 76, 60),
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand
        };
        btnRemove.FlatAppearance.BorderSize = 0;
        btnRemove.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 231, 76, 60);

        var capturedInd = indicator;
        btnRemove.Click += (s, e) =>
        {
            Indicators?.Remove(capturedInd);
            RefreshUI();
            Changed?.Invoke();
        };
        row.Controls.Add(btnRemove);

        // Parameters
        int y = 26;
        foreach (var param in indicator.Parameters)
        {
            var lblParam = new Label
            {
                Text = param.Name,
                Location = new Point(10, y + 2),
                Size = new Size(55, 16),
                ForeColor = Color.FromArgb(150, 155, 165),
                Font = new Font("Segoe UI", 8f)
            };

            var txtParam = new TextBox
            {
                Text = param.Value.ToString("G"),
                Location = new Point(68, y),
                Size = new Size(55, 20),
                BackColor = Color.FromArgb(42, 46, 57),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f),
                Tag = param
            };

            var capturedParam = param;
            var capturedTxt = txtParam;
            var capturedLbl = lblName;
            var capturedIndForParam = indicator;

            void ApplyParam()
            {
                if (double.TryParse(capturedTxt.Text, out var val) && val != capturedParam.Value)
                {
                    capturedParam.Value = val;
                    capturedLbl.Text = capturedIndForParam.ShortName;
                    Changed?.Invoke();
                }
            }

            txtParam.Leave += (s, e) => ApplyParam();
            txtParam.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { ApplyParam(); e.SuppressKeyPress = true; }
            };

            // Min/Max label
            var lblRange = new Label
            {
                Text = $"({param.Min}-{param.Max})",
                Location = new Point(128, y + 2),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 150, 155, 165),
                Font = new Font("Segoe UI", 7f)
            };

            row.Controls.Add(lblParam);
            row.Controls.Add(txtParam);
            row.Controls.Add(lblRange);
            y += 22;
        }

        return row;
    }
}
