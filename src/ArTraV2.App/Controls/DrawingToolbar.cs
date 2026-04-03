using ArTraV2.Core.Chart.Drawing;

namespace ArTraV2.App.Controls;

public class DrawingToolbar : Panel
{
    public event Action<DrawingObjectType?>? ToolSelected;

    private readonly Button[] _buttons;
    private Button? _activeButton;

    private static readonly (string Label, DrawingObjectType? Type)[] Tools =
    [
        ("\u271B", null),                                  // Crosshair
        ("\u2571", DrawingObjectType.TrendLine),            // Trend Line
        ("\u2500", DrawingObjectType.HorizontalLine),       // Horizontal Line
        ("Fib", DrawingObjectType.FibonacciRetracement),   // Fibonacci
        ("\u25AD", DrawingObjectType.Rectangle),            // Rectangle
        ("T", DrawingObjectType.TextLabel),                // Text
    ];

    public DrawingToolbar()
    {
        Width = 36;
        Dock = DockStyle.Left;
        BackColor = Color.FromArgb(25, 29, 40);

        _buttons = new Button[Tools.Length];
        for (int i = 0; i < Tools.Length; i++)
        {
            var (label, type) = Tools[i];
            var btn = new Button
            {
                Text = label,
                Size = new Size(32, 32),
                Location = new Point(2, 4 + i * 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(180, 185, 195),
                BackColor = Color.FromArgb(25, 29, 40),
                Font = new Font("Segoe UI", 10f),
                Tag = type
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 46, 57);

            var capturedType = type;
            btn.Click += (s, e) => SelectTool(btn, capturedType);

            _buttons[i] = btn;
            Controls.Add(btn);
        }

        // Default: crosshair
        SelectTool(_buttons[0], null);
    }

    public void SelectTool(Button btn, DrawingObjectType? type)
    {
        if (_activeButton != null)
            _activeButton.BackColor = Color.FromArgb(25, 29, 40);

        _activeButton = btn;
        btn.BackColor = Color.FromArgb(42, 46, 57);
        ToolSelected?.Invoke(type);
    }

    public void ResetToCrosshair()
    {
        SelectTool(_buttons[0], null);
    }
}
