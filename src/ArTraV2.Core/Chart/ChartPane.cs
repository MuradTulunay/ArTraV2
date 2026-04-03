using System.Drawing;
using ArTraV2.Core.Indicators;

namespace ArTraV2.Core.Chart;

public class ChartPane
{
    public string Title { get; set; } = "";
    public bool IsMainPane { get; set; }
    public float HeightRatio { get; set; } = 1f;
    public Rectangle Bounds { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double[] ReferenceLines { get; set; } = [];
    public List<IndicatorResult> Series { get; set; } = [];

    public float PriceToY(double price)
    {
        if (YMax <= YMin) return Bounds.Top;
        return (float)(Bounds.Bottom - (price - YMin) / (YMax - YMin) * Bounds.Height);
    }

    public double YToPrice(float y)
    {
        if (Bounds.Height == 0) return YMin;
        return YMax - (y - Bounds.Top) / (double)Bounds.Height * (YMax - YMin);
    }

    public bool ContainsY(float y) => y >= Bounds.Top && y <= Bounds.Bottom;

    public void AutoScale(double[] visibleValues)
    {
        double min = double.MaxValue, max = double.MinValue;
        foreach (var v in visibleValues)
        {
            if (double.IsNaN(v)) continue;
            if (v < min) min = v;
            if (v > max) max = v;
        }

        if (min == double.MaxValue) { YMin = 0; YMax = 100; return; }

        var padding = (max - min) * 0.05;
        if (padding == 0) padding = max * 0.01;
        YMin = min - padding;
        YMax = max + padding;
    }
}
