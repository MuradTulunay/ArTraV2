using System.Drawing;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Indicators.Impl;

public class SmaIndicator : IIndicator
{
    public string Name => "Simple Moving Average";
    public string ShortName => $"SMA({Period})";
    public bool IsOverlay => true;
    public double[] ReferenceLines => [];

    public IndicatorParameter[] Parameters { get; } =
    [
        new("Period", 20, 1, 500)
    ];

    private int Period => (int)Parameters[0].Value;

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        var values = new double[data.Count];
        Array.Fill(values, double.NaN);

        if (data.Count < Period) return [new(ShortName, values, Color.FromArgb(255, 152, 0))];

        double sum = 0;
        for (int i = 0; i < Period; i++)
            sum += data[i].Close;

        values[Period - 1] = sum / Period;

        for (int i = Period; i < data.Count; i++)
        {
            sum += data[i].Close - data[i - Period].Close;
            values[i] = sum / Period;
        }

        return [new(ShortName, values, Color.FromArgb(255, 152, 0))];
    }
}
