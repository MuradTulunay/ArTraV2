using ArTraV2.Core.Indicators;

namespace ArTraV2.App.Controls;

public class ActiveIndicatorsPanel : Panel
{
    private readonly FlowLayoutPanel _flow = new();
    public event Action? Changed;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<IIndicator> Indicators { get; set; } = [];

    public ActiveIndicatorsPanel()
    {
        Dock = DockStyle.Right;
        Width = 220;
        BackColor = Color.FromArgb(25, 29, 40);
        Visible = false;

        var header = new Label
        {
            Text = "Active Indicators",
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = Color.FromArgb(150, 155, 165),
            BackColor = Color.FromArgb(30, 34, 45),
            Font = new Font("Segoe UI Semibold", 9f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
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

    public void Refresh(List<IIndicator> indicators)
    {
        Indicators = indicators;
        _flow.Controls.Clear();
        Visible = indicators.Count > 0;

        foreach (var ind in indicators)
        {
            var row = CreateIndicatorRow(ind);
            _flow.Controls.Add(row);
        }
    }

    private Panel CreateIndicatorRow(IIndicator indicator)
    {
        var row = new Panel
        {
            Width = 205,
            Height = 60,
            BackColor = Color.FromArgb(30, 34, 45),
            Margin = new Padding(0, 2, 0, 2)
        };

        // Name label
        var lblName = new Label
        {
            Text = indicator.ShortName,
            Location = new Point(6, 4),
            AutoSize = true,
            ForeColor = Color.FromArgb(41, 98, 255),
            Font = new Font("Segoe UI Semibold", 9f)
        };
        row.Controls.Add(lblName);

        // Remove button
        var btnRemove = new Button
        {
            Text = "X",
            Size = new Size(22, 22),
            Location = new Point(178, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 231, 76, 60),
            ForeColor = Color.FromArgb(231, 76, 60),
            Font = new Font("Segoe UI", 8f)
        };
        btnRemove.FlatAppearance.BorderSize = 0;
        var capturedInd = indicator;
        btnRemove.Click += (s, e) =>
        {
            Indicators.Remove(capturedInd);
            Refresh(Indicators);
            Changed?.Invoke();
        };
        row.Controls.Add(btnRemove);

        // Parameters
        int y = 24;
        foreach (var param in indicator.Parameters)
        {
            if (y > 55) break; // max visible params

            var lblParam = new Label
            {
                Text = param.Name + ":",
                Location = new Point(6, y),
                Size = new Size(50, 18),
                ForeColor = Color.FromArgb(150, 155, 165),
                Font = new Font("Segoe UI", 8f)
            };

            var txtParam = new TextBox
            {
                Text = param.Value.ToString("G"),
                Location = new Point(58, y - 2),
                Size = new Size(50, 18),
                BackColor = Color.FromArgb(42, 46, 57),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8f),
                Tag = param
            };

            var capturedParam = param;
            txtParam.Leave += (s, e) =>
            {
                if (double.TryParse(txtParam.Text, out var val))
                {
                    capturedParam.Value = val;
                    lblName.Text = indicator.ShortName; // refresh name with new params
                    Changed?.Invoke();
                }
            };
            txtParam.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (double.TryParse(txtParam.Text, out var val))
                    {
                        capturedParam.Value = val;
                        lblName.Text = indicator.ShortName;
                        Changed?.Invoke();
                    }
                }
            };

            row.Controls.Add(lblParam);
            row.Controls.Add(txtParam);
            y += 20;
        }

        if (indicator.Parameters.Length > 0)
            row.Height = Math.Max(46, 26 + indicator.Parameters.Length * 20);

        return row;
    }
}
