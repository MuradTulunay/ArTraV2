namespace ArTraV2.Core.Indicators;

public record IndicatorParameter(string Name, double Value, double Min = 1, double Max = 500)
{
    public double Value { get; set; } = Value;
}
