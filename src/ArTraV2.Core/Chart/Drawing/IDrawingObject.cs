using System.Drawing;

namespace ArTraV2.Core.Chart.Drawing;

public interface IDrawingObject
{
    DrawingObjectType Type { get; }
    List<DrawingAnchor> Anchors { get; }
    int RequiredAnchors { get; }
    bool IsComplete { get; }
    Color Color { get; set; }
    float LineWidth { get; set; }
    string? Text { get; set; }
    bool HitTest(PointF screenPoint, float tolerance, Func<DrawingAnchor, PointF> toScreen);
    int HitTestAnchor(PointF screenPoint, float tolerance, Func<DrawingAnchor, PointF> toScreen);
    void Render(Graphics g, Func<DrawingAnchor, PointF> toScreen, bool selected);
}
