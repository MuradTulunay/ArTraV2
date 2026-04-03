using System.Drawing;
using ArTraV2.Core.Indicators;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Formula;

/// <summary>
/// Adapts a FormulaBase (EFC-style indicator) to the IIndicator interface
/// used by ChartRenderer for rendering.
/// </summary>
public class FormulaIndicatorAdapter : IIndicator
{
    private readonly FormulaBase _formula;

    public string Name => _formula.LongName;
    public string ShortName => _formula.GetType().Name;
    public bool IsOverlay => _formula.IsMainView || IsOverlayByName(ShortName);
    public double[] ReferenceLines => GetReferenceLines();

    public IndicatorParameter[] Parameters =>
        _formula.Params.Select(p => new IndicatorParameter(p.Name,
            double.TryParse(p.DefaultValue, out var v) ? v : 0,
            double.TryParse(p.MinValue, out var mn) ? mn : 0,
            double.TryParse(p.MaxValue, out var mx) ? mx : 500)).ToArray();

    public FormulaIndicatorAdapter(FormulaBase formula)
    {
        _formula = formula;
    }

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        if (data.Count == 0) return [];

        var dp = new BarDataProvider(data);
        _formula.DataProvider = dp;

        try
        {
            var package = _formula.Run(dp);
            if (package?.DataArray == null) return [];

            var results = new List<IndicatorResult>();
            var colors = GetColors();

            for (int i = 0; i < package.DataArray.Length; i++)
            {
                var fd = package.DataArray[i];
                if (fd?.Data == null) continue;

                var color = i < colors.Length ? colors[i] : GetDefaultColor(i);
                var renderType = MapRenderType(fd);

                results.Add(new IndicatorResult(
                    fd.Name ?? $"Line{i + 1}",
                    fd.Data,
                    color,
                    1.5f,
                    renderType));
            }

            return results;
        }
        catch
        {
            return [];
        }
    }

    private static IndicatorRenderType MapRenderType(FormulaData fd)
    {
        var attrs = fd.Attrs?.ToUpperInvariant() ?? "";
        if (attrs.Contains("COLORSTICK") || attrs.Contains("VOLSTICK") || fd.RenderType == FormulaRenderType.COLORSTICK)
            return IndicatorRenderType.Histogram;
        return IndicatorRenderType.Line;
    }

    private double[] GetReferenceLines()
    {
        var name = ShortName.ToUpperInvariant();
        if (name.Contains("RSI")) return [30, 70];
        if (name.Contains("MACD") || name.Contains("VMACD")) return [0];
        if (name.Contains("CCI")) return [-100, 100];
        if (name.Contains("WR")) return [-20, -80];
        if (name.Contains("STOCH") || name.Contains("STO")) return [20, 80];
        return [];
    }

    private static bool IsOverlayByName(string name)
    {
        var upper = name.ToUpperInvariant();
        return upper is "MA" or "MA4" or "EMA" or "EMA4" or "BOL" or "BBIBOLL"
            or "ENV" or "SAR" or "ICHIMOKU" or "SR" or "MIKE" or "CDP"
            or "MAIN" or "HL" or "COMPARESTOCK" or "HHLLV"
            || upper.StartsWith("MA") && upper.Length <= 3;
    }

    private Color[] GetColors()
    {
        return
        [
            Color.FromArgb(41, 98, 255),     // Blue
            Color.FromArgb(255, 152, 0),     // Orange
            Color.FromArgb(156, 39, 176),    // Purple
            Color.FromArgb(38, 166, 91),     // Green
            Color.FromArgb(231, 76, 60),     // Red
            Color.FromArgb(0, 188, 212),     // Cyan
            Color.FromArgb(255, 235, 59),    // Yellow
            Color.FromArgb(233, 30, 99),     // Pink
        ];
    }

    private static Color GetDefaultColor(int index)
    {
        var hue = (index * 137) % 360;
        return ColorFromHSL(hue, 0.7, 0.6);
    }

    private static Color ColorFromHSL(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        double m = l - c / 2;
        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }
        return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
    }
}
