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
    private readonly IndicatorParameter[] _params;

    public string Name => _formula.LongName;
    public string ShortName
    {
        get
        {
            var name = _formula.GetType().Name;
            if (_params.Length > 0)
            {
                var vals = string.Join(",", _params.Select(p => p.Value.ToString("G")));
                return $"{name}({vals})";
            }
            return name;
        }
    }

    public bool IsOverlay => _formula.IsMainView || IsOverlayByName(_formula.GetType().Name);
    public double[] ReferenceLines => GetReferenceLines();
    public IndicatorParameter[] Parameters => _params;

    public FormulaIndicatorAdapter(FormulaBase formula)
    {
        _formula = formula;

        // Build mutable parameter list from formula params
        _params = formula.Params.Select(p => new IndicatorParameter(
            p.Name,
            double.TryParse(p.DefaultValue, out var v) ? v : 0,
            double.TryParse(p.MinValue, out var mn) ? mn : 0,
            double.TryParse(p.MaxValue, out var mx) ? mx : 500
        )).ToArray();
    }

    public List<IndicatorResult> Calculate(List<BarData> data)
    {
        if (data.Count == 0) return [];

        // Sync parameters back to formula fields before calculation
        SyncParamsToFormula();

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

                // Skip if all NaN
                bool hasData = false;
                for (int j = 0; j < fd.Data.Length; j++)
                    if (!double.IsNaN(fd.Data[j])) { hasData = true; break; }
                if (!hasData) continue;

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Formula {_formula.GetType().Name} error: {ex.Message}");
            return [];
        }
    }

    private void SyncParamsToFormula()
    {
        var type = _formula.GetType();
        for (int i = 0; i < _params.Length && i < _formula.Params.Count; i++)
        {
            var field = type.GetField(_formula.Params[i].Name);
            if (field != null && field.FieldType == typeof(double))
            {
                field.SetValue(_formula, _params[i].Value);
            }
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
        var name = _formula.GetType().Name.ToUpperInvariant();
        if (name.Contains("RSI") && !name.Contains("VRSI")) return [30, 70];
        if (name is "MACD" or "VMACD") return [0];
        if (name is "CCI") return [-100, 100];
        if (name is "WR") return [-20, -80];
        if (name.Contains("STO") || name is "SO" or "LWR") return [20, 80];
        if (name is "MTM" or "ROC" or "BIAS" or "B3612" or "DBCD" or "DPO" or "SRDM") return [0];
        if (name is "AD" or "MI" or "MICD" or "RC" or "RCCD" or "CMF") return [0];
        if (name is "PSY") return [25, 75];
        return [];
    }

    private static bool IsOverlayByName(string name)
    {
        var upper = name.ToUpperInvariant();
        return upper is "MA" or "MA4" or "EMA" or "EMA4" or "EXPMA"
            or "BOL" or "BBIBOLL" or "BBWIDTH"
            or "ENV" or "SAR" or "ICHIMOKU" or "T_ICHIMOKU"
            or "SR" or "MIKE" or "CDP" or "HHLLV" or "HHV"
            or "MAIN" or "HL" or "COMPARESTOCK" or "DOTLINE" or "OVERLAYV"
            or "LINREGR" or "ZIGW" or "ZIGSR" or "ZIGICON"
            or "BBI" or "DMA"
            || (upper.StartsWith("MA") && upper.Length <= 4 && !upper.Contains("MACD"));
    }

    private Color[] GetColors()
    {
        return
        [
            Color.FromArgb(41, 98, 255),
            Color.FromArgb(255, 152, 0),
            Color.FromArgb(156, 39, 176),
            Color.FromArgb(38, 166, 91),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(0, 188, 212),
            Color.FromArgb(255, 235, 59),
            Color.FromArgb(233, 30, 99),
        ];
    }

    private static Color GetDefaultColor(int index)
    {
        var hue = (index * 137) % 360;
        double c = 0.7 * 0.8;
        double x = c * (1 - Math.Abs(hue / 60.0 % 2 - 1));
        double m = 0.6 - c / 2;
        double r, g, b;
        if (hue < 60) { r = c; g = x; b = 0; }
        else if (hue < 120) { r = x; g = c; b = 0; }
        else if (hue < 180) { r = 0; g = c; b = x; }
        else if (hue < 240) { r = 0; g = x; b = c; }
        else if (hue < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }
        return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
    }
}
