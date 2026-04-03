using System.Drawing;

namespace ArTraV2.Core.Indicators;

public record IndicatorResult(
    string Name,
    double[] Values,
    Color Color,
    float LineWidth = 1.5f,
    IndicatorRenderType RenderType = IndicatorRenderType.Line
);

public enum IndicatorRenderType
{
    Line,
    Histogram,
    FilledArea,
    DottedLine
}
