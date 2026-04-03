namespace ArTraV2.Core.Models;

public record BarData
{
    public DateTime Date { get; init; }
    public double Open { get; init; }
    public double High { get; init; }
    public double Low { get; init; }
    public double Close { get; init; }
    public double Volume { get; init; }
    public double AdjClose { get; init; }
}
