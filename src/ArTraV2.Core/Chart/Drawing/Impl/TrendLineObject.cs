using System.Drawing;
using System.Drawing.Drawing2D;

namespace ArTraV2.Core.Chart.Drawing.Impl;

public class TrendLineObject : IDrawingObject
{
    public DrawingObjectType Type => DrawingObjectType.TrendLine;
    public List<DrawingAnchor> Anchors { get; } = [];
    public int RequiredAnchors => 2;
    public bool IsComplete => Anchors.Count >= 2;
    public Color Color { get; set; } = Color.FromArgb(255, 152, 0);
    public float LineWidth { get; set; } = 1.5f;
    public string? Text { get; set; }

    public bool HitTest(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (!IsComplete) return false;
        var p1 = toScreen(Anchors[0]);
        var p2 = toScreen(Anchors[1]);
        return DistanceToSegment(pt, p1, p2) <= tolerance;
    }

    public int HitTestAnchor(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        for (int i = 0; i < Anchors.Count; i++)
        {
            var sp = toScreen(Anchors[i]);
            if (Math.Abs(pt.X - sp.X) <= tolerance && Math.Abs(pt.Y - sp.Y) <= tolerance)
                return i;
        }
        return -1;
    }

    public void Render(Graphics g, Func<DrawingAnchor, PointF> toScreen, bool selected)
    {
        if (Anchors.Count < 2) return;
        var p1 = toScreen(Anchors[0]);
        var p2 = toScreen(Anchors[1]);

        using var pen = new Pen(Color, selected ? LineWidth + 1 : LineWidth);
        g.DrawLine(pen, p1, p2);

        if (selected) DrawHandles(g, [p1, p2]);
    }

    private static void DrawHandles(Graphics g, PointF[] points)
    {
        using var brush = new SolidBrush(Color.White);
        foreach (var p in points)
            g.FillRectangle(brush, p.X - 3, p.Y - 3, 6, 6);
    }

    private static float DistanceToSegment(PointF p, PointF a, PointF b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lenSq = dx * dx + dy * dy;
        if (lenSq == 0) return (float)Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        var t = Math.Max(0, Math.Min(1, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq));
        var projX = a.X + t * dx;
        var projY = a.Y + t * dy;
        return (float)Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
    }
}
