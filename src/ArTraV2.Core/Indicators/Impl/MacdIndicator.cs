using System.Drawing;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Indicators.Impl;

public class MacdIndicator : IIndicator
{
    public string Name => "MACD";
    public string ShortName => $"MACD({Fast},{Slow},{Signal})";
    public bool IsOverlay => false;
    public double[] ReferenceLines => [0];

    public IndicatorParameter[] Parameters { get; } =
    [
        new("Fast", 12, 2, 100),
        new("Slow", 26, 2, 200),
        new("Signal", 9, 2, 50)
    ];

    private int Fast => (int)Parameters[0].Value;
    private int Slow => (int)Parameters[1].Value;
    private int Signal => (int)Parameters[2].Value;

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        var macdLine = new double[data.Count];
        var signalLine = new double[data.Count];
        var histogram = new double[data.Count];
        Array.Fill(macdLine, double.NaN);
        Array.Fill(signalLine, double.NaN);
        Array.Fill(histogram, double.NaN);

        if (data.Count <= Slow)
            return
            [
                new("MACD", macdLine, Color.FromArgb(41, 98, 255)),
                new("Signal", signalLine, Color.FromArgb(255, 152, 0)),
                new("Histogram", histogram, Color.FromArgb(38, 166, 91), 1f, IndicatorRenderType.Histogram)
            ];

        var fastEma = CalculateEma(data, Fast);
        var slowEma = CalculateEma(data, Slow);

        // MACD line = Fast EMA - Slow EMA
        int startIdx = Slow - 1;
        for (int i = startIdx; i < data.Count; i++)
        {
            if (!double.IsNaN(fastEma[i]) && !double.IsNaN(slowEma[i]))
                macdLine[i] = fastEma[i] - slowEma[i];
        }

        // Signal line = EMA of MACD line
        double multiplier = 2.0 / (Signal + 1);
        int signalStart = -1;
        double sum = 0;
        int count = 0;

        for (int i = startIdx; i < data.Count && count < Signal; i++)
        {
            if (!double.IsNaN(macdLine[i]))
            {
                sum += macdLine[i];
                count++;
                if (count == Signal)
                {
                    signalLine[i] = sum / Signal;
                    signalStart = i;
                }
            }
        }

        if (signalStart > 0)
        {
            for (int i = signalStart + 1; i < data.Count; i++)
            {
                if (!double.IsNaN(macdLine[i]))
                    signalLine[i] = (macdLine[i] - signalLine[i - 1]) * multiplier + signalLine[i - 1];
            }
        }

        // Histogram = MACD - Signal
        for (int i = 0; i < data.Count; i++)
        {
            if (!double.IsNaN(macdLine[i]) && !double.IsNaN(signalLine[i]))
                histogram[i] = macdLine[i] - signalLine[i];
        }

        return
        [
            new("MACD", macdLine, Color.FromArgb(41, 98, 255)),
            new("Signal", signalLine, Color.FromArgb(255, 152, 0)),
            new("Histogram", histogram, Color.FromArgb(38, 166, 91), 1f, IndicatorRenderType.Histogram)
        ];
    }

    private static double[] CalculateEma(List<BarData> data, int period)
    {
        var values = new double[data.Count];
        Array.Fill(values, double.NaN);

        if (data.Count < period) return values;

        double multiplier = 2.0 / (period + 1);
        double sum = 0;

        for (int i = 0; i < period; i++)
            sum += data[i].Close;

        values[period - 1] = sum / period;

        for (int i = period; i < data.Count; i++)
            values[i] = (data[i].Close - values[i - 1]) * multiplier + values[i - 1];

        return values;
    }
}
