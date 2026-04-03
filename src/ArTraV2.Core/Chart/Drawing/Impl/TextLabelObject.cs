using System.Drawing;

namespace ArTraV2.Core.Chart.Drawing.Impl;

public class TextLabelObject : IDrawingObject
{
    public DrawingObjectType Type => DrawingObjectType.TextLabel;
    public List<DrawingAnchor> Anchors { get; } = [];
    public int RequiredAnchors => 1;
    public bool IsComplete => Anchors.Count >= 1;
    public Color Color { get; set; } = Color.FromArgb(209, 212, 220);
    public float LineWidth { get; set; } = 1f;
    public string? Text { get; set; } = "Label";

    public bool HitTest(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (!IsComplete) return false;
        var sp = toScreen(Anchors[0]);
        return Math.Abs(pt.X - sp.X) <= 40 + tolerance && Math.Abs(pt.Y - sp.Y) <= 10 + tolerance;
    }

    public int HitTestAnchor(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (Anchors.Count == 0) return -1;
        var sp = toScreen(Anchors[0]);
        if (Math.Abs(pt.X - sp.X) <= tolerance && Math.Abs(pt.Y - sp.Y) <= tolerance)
            return 0;
        return -1;
    }

    public void Render(Graphics g, Func<DrawingAnchor, PointF> toScreen, bool selected)
    {
        if (!IsComplete || string.IsNullOrEmpty(Text)) return;
        var sp = toScreen(Anchors[0]);

        using var font = new Font("Segoe UI", 10f);
        using var brush = new SolidBrush(Color);
        var sz = g.MeasureString(Text, font);

        if (selected)
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(40, Color));
            g.FillRectangle(bgBrush, sp.X - 2, sp.Y - 2, sz.Width + 4, sz.Height + 4);
        }

        g.DrawString(Text, font, brush, sp.X, sp.Y);
    }
}
