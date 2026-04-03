using System.Drawing;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Indicators.Impl;

public class BollingerBandsIndicator : IIndicator
{
    public string Name => "Bollinger Bands";
    public string ShortName => $"BB({Period},{StdDev:F1})";
    public bool IsOverlay => true;
    public double[] ReferenceLines => [];

    public IndicatorParameter[] Parameters { get; } =
    [
        new("Period", 20, 2, 200),
        new("StdDev", 2, 0.5, 5)
    ];

    private int Period => (int)Parameters[0].Value;
    private double StdDev => Parameters[1].Value;

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        var upper = new double[data.Count];
        var middle = new double[data.Count];
        var lower = new double[data.Count];
        Array.Fill(upper, double.NaN);
        Array.Fill(middle, double.NaN);
        Array.Fill(lower, double.NaN);

        var bandColor = Color.FromArgb(100, 41, 98, 255);

        if (data.Count < Period)
            return
            [
                new("BB Upper", upper, bandColor),
                new("BB Middle", middle, Color.FromArgb(41, 98, 255)),
                new("BB Lower", lower, bandColor)
            ];

        for (int i = Period - 1; i < data.Count; i++)
        {
            double sum = 0;
            for (int j = i - Period + 1; j <= i; j++)
                sum += data[j].Close;

            double sma = sum / Period;
            middle[i] = sma;

            double sumSq = 0;
            for (int j = i - Period + 1; j <= i; j++)
            {
                var diff = data[j].Close - sma;
                sumSq += diff * diff;
            }

            double std = Math.Sqrt(sumSq / Period);
            upper[i] = sma + StdDev * std;
            lower[i] = sma - StdDev * std;
        }

        return
        [
            new("BB Upper", upper, bandColor, 1f, IndicatorRenderType.DottedLine),
            new("BB Middle", middle, Color.FromArgb(41, 98, 255), 1f),
            new("BB Lower", lower, bandColor, 1f, IndicatorRenderType.DottedLine)
        ];
    }
}
