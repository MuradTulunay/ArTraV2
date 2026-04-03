using System.Drawing;
using System.Drawing.Drawing2D;

namespace ArTraV2.Core.Chart.Drawing.Impl;

public class FibonacciRetracementObject : IDrawingObject
{
    public DrawingObjectType Type => DrawingObjectType.FibonacciRetracement;
    public List<DrawingAnchor> Anchors { get; } = [];
    public int RequiredAnchors => 2;
    public bool IsComplete => Anchors.Count >= 2;
    public Color Color { get; set; } = Color.FromArgb(255, 152, 0);
    public float LineWidth { get; set; } = 1f;
    public string? Text { get; set; }

    private static readonly double[] Levels = [0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0];
    private static readonly Color[] LevelColors =
    [
        Color.FromArgb(40, 255, 152, 0),
        Color.FromArgb(30, 255, 152, 0),
        Color.FromArgb(25, 255, 152, 0),
        Color.FromArgb(20, 255, 152, 0),
        Color.FromArgb(25, 255, 152, 0),
        Color.FromArgb(30, 255, 152, 0),
        Color.FromArgb(40, 255, 152, 0)
    ];

    public bool HitTest(PointF pt, float tolerance, Func<DrawingAnchor, PointF> toScreen)
    {
        if (!IsComplete) return false;
        var p1 = toScreen(Anchors[0]);
        var p2 = toScreen(Anchors[1]);
        var minY = Math.Min(p1.Y, p2.Y);
        var maxY = Math.Max(p1.Y, p2.Y);
        return pt.Y >= minY - tolerance && pt.Y <= maxY + tolerance;
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

        var highPrice = Math.Max(Anchors[0].Price, Anchors[1].Price);
        var lowPrice = Math.Min(Anchors[0].Price, Anchors[1].Price);
        var range = highPrice - lowPrice;

        using var pen = new Pen(Color, selected ? LineWidth + 0.5f : LineWidth);
        using var font = new Font("Segoe UI", 7.5f);
        using var textBrush = new SolidBrush(Color);

        var clip = g.ClipBounds;
        var right = clip.Right > 0 ? clip.Right : 2000;

        for (int i = 0; i < Levels.Length; i++)
        {
            var price = highPrice - range * Levels[i];
            var anchor = new DrawingAnchor(0, price);
            var y = toScreen(anchor).Y;

            g.DrawLine(pen, 0, y, right, y);

            var label = $"{Levels[i] * 100:F1}% ({ChartRenderer.FormatPrice(price)})";
            g.DrawString(label, font, textBrush, 4, y - 14);

            // Fill between levels
            if (i < Levels.Length - 1)
            {
                var nextPrice = highPrice - range * Levels[i + 1];
                var nextY = toScreen(new DrawingAnchor(0, nextPrice)).Y;
                var fillRect = new RectangleF(0, Math.Min(y, nextY), right, Math.Abs(nextY - y));
                using var fill = new SolidBrush(LevelColors[i]);
                g.FillRectangle(fill, fillRect);
            }
        }

        if (selected)
        {
            var p1 = toScreen(Anchors[0]);
            var p2 = toScreen(Anchors[1]);
            using var handleBrush = new SolidBrush(Color.White);
            g.FillRectangle(handleBrush, p1.X - 3, p1.Y - 3, 6, 6);
            g.FillRectangle(handleBrush, p2.X - 3, p2.Y - 3, 6, 6);
        }
    }
}
