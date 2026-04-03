using System.Drawing;
using System.Drawing.Drawing2D;

namespace ArTraV2.Core.Chart.Drawing.Impl;

public class HorizontalLineObject : IDrawingObject
{
    public DrawingObjectType Type => DrawingObjectType.HorizontalLine;
    public List<DrawingAnchor> Anchors { get; } = [];
    public int RequiredAnchors => 1;
    public bool IsComplete => Anchors.Count >= 1;
    public Color Color { get; set; } = Color.FromArgb(41, 98, 255);
    public float LineWidth { get; set; } = 1f;
    public string? Text { get; set; }

    public bool HitTest(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (!IsComplete) return false;
        var sp = toScreen(Anchors[0]);
        return Math.Abs(pt.Y - sp.Y) <= tolerance;
    }

    public int HitTestAnchor(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (Anchors.Count == 0) return -1;
        var sp = toScreen(Anchors[0]);
        if (Math.Abs(pt.Y - sp.Y) <= tolerance) return 0;
        return -1;
    }

    public void Render(Graphics g, Func<DrawingAnchor, PointF> toScreen, bool selected)
    {
        if (!IsComplete) return;
        var sp = toScreen(Anchors[0]);

        using var pen = new Pen(Color, selected ? LineWidth + 1 : LineWidth) { DashStyle = DashStyle.Dash };
        // Draw across full width — use clip bounds
        var clip = g.ClipBounds;
        g.DrawLine(pen, 0, sp.Y, clip.Right > 0 ? clip.Right : 2000, sp.Y);

        // Price label
        using var font = new Font("Segoe UI", 7.5f);
        using var brush = new SolidBrush(Color);
        g.DrawString(ChartRenderer.FormatPrice(Anchors[0].Price), font, brush, clip.Right - 65, sp.Y - 12);
    }
}
