using System.Drawing;

namespace ArTraV2.Core.Chart.Drawing.Impl;

public class RectangleObject : IDrawingObject
{
    public DrawingObjectType Type => DrawingObjectType.Rectangle;
    public List<DrawingAnchor> Anchors { get; } = [];
    public int RequiredAnchors => 2;
    public bool IsComplete => Anchors.Count >= 2;
    public Color Color { get; set; } = Color.FromArgb(41, 98, 255);
    public float LineWidth { get; set; } = 1f;
    public string? Text { get; set; }

    public bool HitTest(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (!IsComplete) return false;
        var p1 = toScreen(Anchors[0]);
        var p2 = toScreen(Anchors[1]);
        var rect = NormalizeRect(p1, p2);
        rect.Inflate(tolerance, tolerance);
        return rect.Contains(pt);
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
        if (!IsComplete) return;
        var p1 = toScreen(Anchors[0]);
        var p2 = toScreen(Anchors[1]);
        var rect = NormalizeRect(p1, p2);

        using var fill = new SolidBrush(Color.FromArgb(30, Color));
        g.FillRectangle(fill, rect);

        using var pen = new Pen(Color, selected ? LineWidth + 1 : LineWidth);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        if (selected)
        {
            using var handleBrush = new SolidBrush(Color.White);
            g.FillRectangle(handleBrush, p1.X - 3, p1.Y - 3, 6, 6);
            g.FillRectangle(handleBrush, p2.X - 3, p2.Y - 3, 6, 6);
        }
    }

    private static RectangleF NormalizeRect(PointF p1, PointF p2)
    {
        var x = Math.Min(p1.X, p2.X);
        var y = Math.Min(p1.Y, p2.Y);
        return new RectangleF(x, y, Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
    }
}
