using System.Drawing;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Indicators.Impl;

public class RsiIndicator : IIndicator
{
    public string Name => "Relative Strength Index";
    public string ShortName => $"RSI({Period})";
    public bool IsOverlay => false;
    public double[] ReferenceLines => [30, 70];

    public IndicatorParameter[] Parameters { get; } =
    [
        new("Period", 14, 2, 100)
    ];

    private int Period => (int)Parameters[0].Value;

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        var values = new double[data.Count];
        Array.Fill(values, double.NaN);

        if (data.Count <= Period) return [new(ShortName, values, Color.FromArgb(156, 39, 176))];

        double avgGain = 0, avgLoss = 0;

        for (int i = 1; i <= Period; i++)
        {
            var change = data[i].Close - data[i - 1].Close;
            if (change > 0) avgGain += change;
            else avgLoss -= change;
        }

        avgGain /= Period;
        avgLoss /= Period;

        values[Period] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);

        for (int i = Period + 1; i < data.Count; i++)
        {
            var change = data[i].Close - data[i - 1].Close;
            var gain = change > 0 ? change : 0;
            var loss = change < 0 ? -change : 0;

            avgGain = (avgGain * (Period - 1) + gain) / Period;
            avgLoss = (avgLoss * (Period - 1) + loss) / Period;

            values[i] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
        }

        return [new(ShortName, values, Color.FromArgb(156, 39, 176))];
    }
}
