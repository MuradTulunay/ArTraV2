using System.Drawing;
using ArTraV2.Core.Chart.Drawing.Impl;

namespace ArTraV2.Core.Chart.Drawing;

public class DrawingManager
{
    public List<IDrawingObject> Objects { get; } = [];
    public DrawingObjectType? ActiveTool { get; set; }
    public IDrawingObject? SelectedObject { get; set; }
    public IDrawingObject? ObjectBeingCreated { get; private set; }

    // Preview: temporary anchor following the mouse during creation
    private DrawingAnchor? _previewAnchor;

    private int _draggingAnchorIndex = -1;
    private bool _isDragging;

    public IDrawingObject CreateObject(DrawingObjectType type)
    {
        return type switch
        {
            DrawingObjectType.TrendLine => new TrendLineObject(),
            DrawingObjectType.HorizontalLine => new HorizontalLineObject(),
            DrawingObjectType.FibonacciRetracement => new FibonacciRetracementObject(),
            DrawingObjectType.Rectangle => new RectangleObject(),
            DrawingObjectType.TextLabel => new TextLabelObject(),
            _ => throw new ArgumentException($"Unknown drawing type: {type}")
        };
    }

    public bool OnMouseDown(PointF screenPos, int barIndex, double price, Func<DrawingAnchor, PointF> toScreen)
    {
        // Creating a new object
        if (ActiveTool.HasValue)
        {
            if (ObjectBeingCreated == null)
            {
                ObjectBeingCreated = CreateObject(ActiveTool.Value);
                SelectedObject = ObjectBeingCreated;
            }

            ObjectBeingCreated.Anchors.Add(new DrawingAnchor(barIndex, price));
            _previewAnchor = null;

            if (ObjectBeingCreated.IsComplete)
            {
                Objects.Add(ObjectBeingCreated);
                SelectedObject = ObjectBeingCreated;
                ObjectBeingCreated = null;
                ActiveTool = null;
                return true; // Tool finished
            }
            return false;
        }

        // Selection mode
        const float tolerance = 6f;

        if (SelectedObject != null)
        {
            var anchorIdx = SelectedObject.HitTestAnchor(screenPos, tolerance, toScreen);
            if (anchorIdx >= 0)
            {
                _draggingAnchorIndex = anchorIdx;
                _isDragging = true;
                return false;
            }
        }

        SelectedObject = null;
        for (int i = Objects.Count - 1; i >= 0; i--)
        {
            if (Objects[i].HitTest(screenPos, tolerance, toScreen))
            {
                SelectedObject = Objects[i];
                break;
            }
        }

        return false;
    }

    public void OnMouseMove(PointF screenPos, int barIndex, double price)
    {
        if (_isDragging && SelectedObject != null && _draggingAnchorIndex >= 0)
        {
            SelectedObject.Anchors[_draggingAnchorIndex] = new DrawingAnchor(barIndex, price);
        }

        // Update preview anchor during object creation
        if (ObjectBeingCreated != null && ObjectBeingCreated.Anchors.Count > 0 &&
            ObjectBeingCreated.Anchors.Count < ObjectBeingCreated.RequiredAnchors)
        {
            _previewAnchor = new DrawingAnchor(barIndex, price);
        }
    }

    public void OnMouseUp()
    {
        _isDragging = false;
        _draggingAnchorIndex = -1;
    }

    public void DeleteSelected()
    {
        if (SelectedObject != null)
        {
            Objects.Remove(SelectedObject);
            SelectedObject = null;
        }
    }

    public void CancelCreation()
    {
        ObjectBeingCreated = null;
        ActiveTool = null;
        _previewAnchor = null;
    }

    public void RenderAll(Graphics g, Func<DrawingAnchor, PointF> toScreen)
    {
        foreach (var obj in Objects)
            obj.Render(g, toScreen, obj == SelectedObject);

        // Render in-progress object with preview
        if (ObjectBeingCreated != null)
        {
            if (_previewAnchor != null && ObjectBeingCreated.Anchors.Count < ObjectBeingCreated.RequiredAnchors)
            {
                // Temporarily add preview anchor for rendering
                ObjectBeingCreated.Anchors.Add(_previewAnchor);
                ObjectBeingCreated.Render(g, toScreen, true);
                ObjectBeingCreated.Anchors.RemoveAt(ObjectBeingCreated.Anchors.Count - 1);
            }
            else
            {
                ObjectBeingCreated.Render(g, toScreen, true);
            }
        }
    }

    public bool IsDrawing => ActiveTool.HasValue || ObjectBeingCreated != null;
    public bool IsDragging => _isDragging;
}
