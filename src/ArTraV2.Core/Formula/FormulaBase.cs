using System.Reflection;

namespace ArTraV2.Core.Formula;

/// <summary>
/// EFC-compatible formula base class with 50+ built-in technical analysis functions.
/// All ArTraNew/EFC indicators inherit from this class.
/// </summary>
[Serializable]
public abstract class FormulaBase
{
    public IFormulaDataProvider? DataProvider { get; set; }
    public List<FormulaParam> Params { get; } = [];
    public virtual string LongName => GetType().Name;
    public virtual string Description => "";
    public virtual string OutputFields => "";
    public bool IsMainView { get; set; }

    // Registry of formula assemblies
    private static readonly Dictionary<string, Assembly> _assemblies = new();
    private static List<Type>? _cachedTypes;

    protected int Count => DataProvider?.Count ?? 0;

    // OHLCV data access
    protected FormulaData OPEN => new(DataProvider!["OPEN"]);
    protected FormulaData HIGH => new(DataProvider!["HIGH"]);
    protected FormulaData LOW => new(DataProvider!["LOW"]);
    protected FormulaData CLOSE => new(DataProvider!["CLOSE"]);
    protected FormulaData VOLUME => new(DataProvider!["VOLUME"]);
    protected FormulaData VOL => VOLUME;
    protected FormulaData DATE => new(DataProvider!["DATE"]);
    protected FormulaData O => OPEN;
    protected FormulaData H => HIGH;
    protected FormulaData L => LOW;
    protected FormulaData C => CLOSE;
    protected FormulaData V => VOLUME;

    protected FormulaData ORGDATA(string name) => new(DataProvider![name]);

    // Parameter management
    protected void AddParam(string name, double defaultVal, double min, double max)
    {
        Params.Add(new FormulaParam { Name = name, DefaultValue = defaultVal.ToString(), MinValue = min.ToString(), MaxValue = max.ToString(), ParamType = FormulaParamType.Double });
    }

    protected void AddParam(string name, string defaultVal, string min, string max, string step, string desc, FormulaParamType type)
    {
        Params.Add(new FormulaParam { Name = name, DefaultValue = defaultVal, MinValue = min, MaxValue = max, Step = step, Description = desc, ParamType = type });
    }

    public abstract FormulaPackage Run(IFormulaDataProvider dp);

    // ========== MOVING AVERAGES ==========

    protected FormulaData MA(FormulaData data, double period)
    {
        int n = (int)period;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (n <= 0 || data.Length < n) return new FormulaData(result);

        double sum = 0;
        for (int i = 0; i < n; i++) sum += data[i];
        result[n - 1] = sum / n;
        for (int i = n; i < data.Length; i++)
        {
            sum += data[i] - data[i - n];
            result[i] = sum / n;
        }
        return new FormulaData(result);
    }

    protected FormulaData EMA(FormulaData data, double period)
    {
        int n = (int)period;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (n <= 0 || data.Length < n) return new FormulaData(result);

        double mult = 2.0 / (n + 1);
        double sum = 0;
        for (int i = 0; i < n; i++) sum += data[i];
        result[n - 1] = sum / n;
        for (int i = n; i < data.Length; i++)
            result[i] = (data[i] - result[i - 1]) * mult + result[i - 1];
        return new FormulaData(result);
    }

    protected FormulaData SMA(FormulaData data, double n, double m)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (data.Length == 0) return new FormulaData(result);
        result[0] = data[0];
        for (int i = 1; i < data.Length; i++)
            result[i] = (data[i] * m + result[i - 1] * (n - m)) / n;
        return new FormulaData(result);
    }

    protected FormulaData DMA(FormulaData data, double a)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (data.Length == 0) return new FormulaData(result);
        result[0] = data[0];
        for (int i = 1; i < data.Length; i++)
            result[i] = data[i] * a + result[i - 1] * (1 - a);
        return new FormulaData(result);
    }

    // ========== REFERENCE FUNCTIONS ==========

    protected FormulaData REF(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = period; i < data.Length; i++)
            result[i] = data[i - period];
        return new FormulaData(result);
    }

    protected FormulaData HHV(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = 0; i < data.Length; i++)
        {
            double max = double.MinValue;
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] > max) max = data[j];
            result[i] = max == double.MinValue ? double.NaN : max;
        }
        return new FormulaData(result);
    }

    protected FormulaData LLV(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = 0; i < data.Length; i++)
        {
            double min = double.MaxValue;
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] < min) min = data[j];
            result[i] = min == double.MaxValue ? double.NaN : min;
        }
        return new FormulaData(result);
    }

    protected FormulaData HHVBARS(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = 0; i < data.Length; i++)
        {
            double max = double.MinValue;
            int maxIdx = i;
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] > max) { max = data[j]; maxIdx = j; }
            result[i] = i - maxIdx;
        }
        return new FormulaData(result);
    }

    protected FormulaData LLVBARS(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = 0; i < data.Length; i++)
        {
            double min = double.MaxValue;
            int minIdx = i;
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] < min) { min = data[j]; minIdx = j; }
            result[i] = i - minIdx;
        }
        return new FormulaData(result);
    }

    // ========== AGGREGATE FUNCTIONS ==========

    protected FormulaData SUM(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (period <= 0) return new FormulaData(result);
        double sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += double.IsNaN(data[i]) ? 0 : data[i];
            if (i >= period) sum -= double.IsNaN(data[i - period]) ? 0 : data[i - period];
            if (i >= period - 1) result[i] = sum;
        }
        return new FormulaData(result);
    }

    protected FormulaData COUNT(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int count = 0;
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] != 0) count++;
            result[i] = count;
        }
        return new FormulaData(result);
    }

    protected FormulaData BARSCOUNT(FormulaData data)
    {
        var result = new double[data.Length];
        for (int i = 0; i < data.Length; i++) result[i] = i + 1;
        return new FormulaData(result);
    }

    protected FormulaData BARSLAST(FormulaData data)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        int last = -1;
        for (int i = 0; i < data.Length; i++)
        {
            if (!double.IsNaN(data[i]) && data[i] != 0) last = i;
            if (last >= 0) result[i] = i - last;
        }
        return new FormulaData(result);
    }

    // ========== STATISTICAL FUNCTIONS ==========

    protected FormulaData STD(FormulaData data, double n)
    {
        int period = (int)n;
        var ma = MA(data, n);
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = period - 1; i < data.Length; i++)
        {
            if (double.IsNaN(ma[i])) continue;
            double sumSq = 0;
            for (int j = i - period + 1; j <= i; j++)
            {
                var diff = data[j] - ma[i];
                sumSq += diff * diff;
            }
            result[i] = Math.Sqrt(sumSq / period);
        }
        return new FormulaData(result);
    }

    protected FormulaData AVEDEV(FormulaData data, double n)
    {
        int period = (int)n;
        var ma = MA(data, n);
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = period - 1; i < data.Length; i++)
        {
            if (double.IsNaN(ma[i])) continue;
            double sumDev = 0;
            for (int j = i - period + 1; j <= i; j++)
                sumDev += Math.Abs(data[j] - ma[i]);
            result[i] = sumDev / period;
        }
        return new FormulaData(result);
    }

    // ========== CONDITIONAL / LOGICAL ==========

    protected FormulaData IF(FormulaData cond, FormulaData trueVal, FormulaData falseVal)
    {
        int len = cond.Length;
        var result = new double[len];
        for (int i = 0; i < len; i++)
            result[i] = (!double.IsNaN(cond[i]) && cond[i] != 0)
                ? (i < trueVal.Length ? trueVal[i] : double.NaN)
                : (i < falseVal.Length ? falseVal[i] : double.NaN);
        return new FormulaData(result);
    }

    protected FormulaData IF(FormulaData cond, double trueVal, FormulaData falseVal)
        => IF(cond, new FormulaData(trueVal, cond.Length), falseVal);
    protected FormulaData IF(FormulaData cond, FormulaData trueVal, double falseVal)
        => IF(cond, trueVal, new FormulaData(falseVal, cond.Length));
    protected FormulaData IF(FormulaData cond, double trueVal, double falseVal)
        => IF(cond, new FormulaData(trueVal, cond.Length), new FormulaData(falseVal, cond.Length));

    protected FormulaData CROSS(FormulaData a, FormulaData b)
    {
        var result = new double[a.Length];
        for (int i = 1; i < a.Length; i++)
            result[i] = (a[i] > b[i] && a[i - 1] <= b[i - 1]) ? 1 : 0;
        return new FormulaData(result);
    }

    protected FormulaData NOT(FormulaData data)
    {
        var result = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (double.IsNaN(data[i]) || data[i] == 0) ? 1 : 0;
        return new FormulaData(result);
    }

    protected FormulaData BETWEEN(FormulaData data, FormulaData low, FormulaData high)
    {
        var result = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (data[i] >= low[i] && data[i] <= high[i]) ? 1 : 0;
        return new FormulaData(result);
    }

    protected FormulaData EVERY(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        for (int i = period - 1; i < data.Length; i++)
        {
            bool all = true;
            for (int j = i - period + 1; j <= i; j++)
                if (double.IsNaN(data[j]) || data[j] == 0) { all = false; break; }
            result[i] = all ? 1 : 0;
        }
        return new FormulaData(result);
    }

    protected FormulaData EXIST(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int start = Math.Max(0, i - period + 1);
            for (int j = start; j <= i; j++)
                if (!double.IsNaN(data[j]) && data[j] != 0) { result[i] = 1; break; }
        }
        return new FormulaData(result);
    }

    protected FormulaData FILTER(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        int skip = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (skip > 0) { skip--; continue; }
            if (!double.IsNaN(data[i]) && data[i] != 0) { result[i] = 1; skip = period; }
        }
        return new FormulaData(result);
    }

    // ========== MATH FUNCTIONS ==========

    protected FormulaData MAX(FormulaData a, FormulaData b) => FormulaData.BinaryOp2(a, b, Math.Max);
    protected FormulaData MIN(FormulaData a, FormulaData b) => FormulaData.BinaryOp2(a, b, Math.Min);
    protected FormulaData MAX(FormulaData a, double b) => FormulaData.UnaryOp2(a, x => Math.Max(x, b));
    protected FormulaData MIN(FormulaData a, double b) => FormulaData.UnaryOp2(a, x => Math.Min(x, b));
    protected FormulaData MAX(FormulaData a, FormulaData b, FormulaData c) => MAX(MAX(a, b), c);
    protected FormulaData MIN(FormulaData a, FormulaData b, FormulaData c) => MIN(MIN(a, b), c);
    protected FormulaData ABS(FormulaData data) => FormulaData.UnaryOp2(data, Math.Abs);
    protected FormulaData SQRT(FormulaData data) => FormulaData.UnaryOp2(data, Math.Sqrt);
    protected FormulaData SQR(FormulaData data) => FormulaData.UnaryOp2(data, x => x * x);
    protected FormulaData LOG(FormulaData data) => FormulaData.UnaryOp2(data, Math.Log10);
    protected FormulaData LN(FormulaData data) => FormulaData.UnaryOp2(data, Math.Log);
    protected FormulaData EXP(FormulaData data) => FormulaData.UnaryOp2(data, Math.Exp);
    protected FormulaData POWER(FormulaData data, double n) => FormulaData.UnaryOp2(data, x => Math.Pow(x, n));
    protected FormulaData FLOOR(FormulaData data) => FormulaData.UnaryOp2(data, Math.Floor);
    protected FormulaData CEILING(FormulaData data) => FormulaData.UnaryOp2(data, Math.Ceiling);
    protected FormulaData ROUND(FormulaData data, int decimals = 0) => FormulaData.UnaryOp2(data, x => Math.Round(x, decimals));
    protected FormulaData SGN(FormulaData data) => FormulaData.UnaryOp2(data, x => Math.Sign(x));
    protected FormulaData SIN(FormulaData data) => FormulaData.UnaryOp2(data, Math.Sin);
    protected FormulaData COS(FormulaData data) => FormulaData.UnaryOp2(data, Math.Cos);
    protected FormulaData TAN(FormulaData data) => FormulaData.UnaryOp2(data, Math.Tan);

    protected FormulaData MAXVALUE(FormulaData data)
    {
        double max = double.MinValue;
        for (int i = 0; i < data.Length; i++)
            if (!double.IsNaN(data[i]) && data[i] > max) max = data[i];
        return new FormulaData(max, data.Length);
    }

    protected FormulaData MINVALUE(FormulaData data)
    {
        double min = double.MaxValue;
        for (int i = 0; i < data.Length; i++)
            if (!double.IsNaN(data[i]) && data[i] < min) min = data[i];
        return new FormulaData(min, data.Length);
    }

    // ========== REGRESSION ==========

    protected FormulaData SLOPE(FormulaData data, double n)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = period - 1; i < data.Length; i++)
        {
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int j = 0; j < period; j++)
            {
                sumX += j;
                sumY += data[i - period + 1 + j];
                sumXY += j * data[i - period + 1 + j];
                sumX2 += j * j;
            }
            result[i] = (period * sumXY - sumX * sumY) / (period * sumX2 - sumX * sumX);
        }
        return new FormulaData(result);
    }

    // ========== ADDITIONAL FUNCTIONS (EFC compat) ==========

    protected FormulaData MOV(FormulaData data, double n, int type = 0) => type switch
    {
        1 => EMA(data, n),
        _ => MA(data, n)
    };

    protected FormulaData HHV(FormulaData data)
    {
        var result = new double[data.Length];
        double max = double.MinValue;
        for (int i = 0; i < data.Length; i++)
        {
            if (!double.IsNaN(data[i]) && data[i] > max) max = data[i];
            result[i] = max == double.MinValue ? double.NaN : max;
        }
        return new FormulaData(result);
    }

    protected FormulaData LLV(FormulaData data)
    {
        var result = new double[data.Length];
        double min = double.MaxValue;
        for (int i = 0; i < data.Length; i++)
        {
            if (!double.IsNaN(data[i]) && data[i] < min) min = data[i];
            result[i] = min == double.MaxValue ? double.NaN : min;
        }
        return new FormulaData(result);
    }

    protected FormulaData BARSSINCE(FormulaData data)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        int lastTrue = -1;
        for (int i = 0; i < data.Length; i++)
        {
            if (!double.IsNaN(data[i]) && data[i] != 0) lastTrue = i;
            if (lastTrue >= 0) result[i] = i - lastTrue;
        }
        return new FormulaData(result);
    }

    protected FormulaData LONGCROSS(FormulaData a, FormulaData b, double n)
    {
        int period = (int)n;
        var result = new double[a.Length];
        for (int i = period; i < a.Length; i++)
        {
            if (a[i] > b[i] && a[i - 1] <= b[i - 1])
            {
                bool allBelow = true;
                for (int j = i - period; j < i; j++)
                    if (a[j] > b[j]) { allBelow = false; break; }
                result[i] = allBelow ? 1 : 0;
            }
        }
        return new FormulaData(result);
    }

    protected FormulaData LAST(FormulaData data, double a, double b)
    {
        var result = new double[data.Length];
        int start = (int)a, end = (int)b;
        for (int i = 0; i < data.Length; i++)
        {
            bool allTrue = true;
            for (int j = Math.Max(0, i - start); j <= Math.Max(0, i - end); j++)
                if (double.IsNaN(data[j]) || data[j] == 0) { allTrue = false; break; }
            result[i] = allTrue ? 1 : 0;
        }
        return new FormulaData(result);
    }

    protected FormulaData BACKSET(FormulaData data, double n = 1)
    {
        int period = (int)n;
        var result = new double[data.Length];
        for (int i = data.Length - 1; i >= 0; i--)
        {
            if (!double.IsNaN(data[i]) && data[i] != 0)
                for (int j = Math.Max(0, i - period + 1); j <= i; j++)
                    result[j] = 1;
        }
        return new FormulaData(result);
    }

    protected FormulaData EXTEND(FormulaData data)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        double lastVal = double.NaN;
        for (int i = 0; i < data.Length; i++)
        {
            if (!double.IsNaN(data[i])) lastVal = data[i];
            result[i] = lastVal;
        }
        return new FormulaData(result);
    }

    protected FormulaData MOD(FormulaData a, FormulaData b) => FormulaData.BinaryOp2(a, b, (x, y) => y == 0 ? double.NaN : x % y);
    protected FormulaData INTPART(FormulaData data) => FormulaData.UnaryOp2(data, x => (int)x);
    protected FormulaData REVERSE(FormulaData data) { var r = (double[])data.Data.Clone(); Array.Reverse(r); return new FormulaData(r); }

    protected FormulaData CORR(FormulaData a, FormulaData b, double n)
    {
        int period = (int)n;
        var result = new double[a.Length];
        Array.Fill(result, double.NaN);
        for (int i = period - 1; i < a.Length; i++)
        {
            double sumA = 0, sumB = 0, sumAB = 0, sumA2 = 0, sumB2 = 0;
            for (int j = i - period + 1; j <= i; j++)
            {
                sumA += a[j]; sumB += b[j]; sumAB += a[j] * b[j];
                sumA2 += a[j] * a[j]; sumB2 += b[j] * b[j];
            }
            var denom = Math.Sqrt((period * sumA2 - sumA * sumA) * (period * sumB2 - sumB * sumB));
            result[i] = denom == 0 ? 0 : (period * sumAB - sumA * sumB) / denom;
        }
        return new FormulaData(result);
    }

    protected FormulaData LR(FormulaData data, double n, double start = 0)
    {
        int period = (int)n;
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        for (int i = period - 1; i < data.Length; i++)
        {
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int j = 0; j < period; j++)
            {
                sumX += j; sumY += data[i - period + 1 + j];
                sumXY += j * data[i - period + 1 + j]; sumX2 += j * j;
            }
            var slope = (period * sumXY - sumX * sumY) / (period * sumX2 - sumX * sumX);
            var intercept = (sumY - slope * sumX) / period;
            result[i] = intercept + slope * (period - 1 + start);
        }
        return new FormulaData(result);
    }

    // Stub functions for rendering/drawing (no-ops in calculation context)
    protected FormulaData DRAWICON(FormulaData cond, FormulaData price, int icon) => cond;
    protected FormulaData DRAWICON(FormulaData cond, FormulaData price, string icon) => cond;
    protected FormulaData DRAWICON(FormulaData cond, FormulaData price, FormulaData icon, string color) => cond;
    protected FormulaData DRAWTEXT(FormulaData cond, FormulaData price, string text) => cond;
    protected FormulaData DRAWNUMBER(FormulaData cond, FormulaData price, FormulaData value) => cond;
    protected FormulaData DRAWNUMBER(FormulaData cond, FormulaData price, FormulaData value, double precision) => cond;
    protected FormulaData DRAWNUMBER(FormulaData cond, FormulaData price, FormulaData value, string format) => cond;
    protected FormulaData STICKLINE(FormulaData cond, FormulaData p1, FormulaData p2, double w, double h) => cond;
    protected FormulaData STICKLINE(FormulaData cond, FormulaData p1, FormulaData p2, double w, string color) => cond;
    protected FormulaData DRAWLINE(FormulaData c1, FormulaData p1, FormulaData c2, FormulaData p2, int extend) => p1;
    protected FormulaData DRAWBAND(FormulaData p1, FormulaData c1, FormulaData p2, FormulaData c2) => p1;
    protected FormulaData POLYLINE(FormulaData cond, FormulaData price) => price;
    protected void SETNAME(FormulaData data, string name) => data.Name = name;
    protected FormulaData SETZERO(FormulaData data) => data;
    protected FormulaData SETZERO(FormulaData data, double startBar) { for (int i = 0; i < Math.Min((int)startBar, data.Length); i++) data[i] = double.NaN; return data; }
    protected FormulaData SETZERO(FormulaData data, FormulaData cond) => data;
    protected FormulaData TOVALUE(FormulaData data, FormulaData target, double pct, double type = 0) => data;

    protected FormulaData ISLASTBAR
    {
        get
        {
            var result = new double[Count];
            if (Count > 0) result[Count - 1] = 1;
            return new FormulaData(result);
        }
    }

    // ZIG / Peak / Trough functions
    protected FormulaData ZIG(FormulaData data, double pct = 5)
    {
        var result = new double[data.Length];
        Array.Fill(result, double.NaN);
        if (data.Length < 2) return new FormulaData(result);
        // Simplified zigzag
        double threshold = pct / 100.0;
        double lastPivot = data[0];
        result[0] = data[0];
        bool isUp = data[1] > data[0];
        for (int i = 1; i < data.Length; i++)
        {
            double change = (data[i] - lastPivot) / lastPivot;
            if ((isUp && change < -threshold) || (!isUp && change > threshold))
            {
                isUp = !isUp;
                lastPivot = data[i];
            }
            result[i] = data[i];
        }
        return new FormulaData(result);
    }

    protected FormulaData FINDPEAK(FormulaData data, double n = 5, double m = 1) => HHV(data, n);
    protected FormulaData FINDTROUGH(FormulaData data, double n = 5, double m = 1) => LLV(data, n);
    protected FormulaData PEAK(FormulaData data, double n = 5, double m = 1) => HHV(data, n);
    protected FormulaData TROUGH(FormulaData data, double n = 5, double m = 1) => LLV(data, n);
    protected FormulaData PEAKBARS(FormulaData data, double n = 5, double m = 1) => HHVBARS(data, n);
    protected FormulaData TROUGHBARS(FormulaData data, double n = 5, double m = 1) => LLVBARS(data, n);
    protected FormulaData ZIGP(FormulaData data, double pct = 5) => ZIG(data, pct);

    // Constants
    protected FormulaData EMPTY => new(double.NaN, Count);
    protected const bool FALSE = false;
    protected const bool TRUE = true;

    // Data access helpers
    protected FormulaData ADVANCE => new(DataProvider!["ADVANCE"]);
    protected FormulaData DECLINE => new(DataProvider!["DECLINE"]);
    protected FormulaData AMOUNT => new(DataProvider!["AMOUNT"]);
    protected FormulaData YEAR { get { var d = DATE; var r = new double[d.Length]; for (int i = 0; i < d.Length; i++) r[i] = DateTime.FromOADate(d[i]).Year; return new FormulaData(r); } }
    protected FormulaData MONTH { get { var d = DATE; var r = new double[d.Length]; for (int i = 0; i < d.Length; i++) r[i] = DateTime.FromOADate(d[i]).Month; return new FormulaData(r); } }
    protected FormulaData DAY { get { var d = DATE; var r = new double[d.Length]; for (int i = 0; i < d.Length; i++) r[i] = DateTime.FromOADate(d[i]).Day; return new FormulaData(r); } }
    protected FormulaData HOUR { get { var d = DATE; var r = new double[d.Length]; for (int i = 0; i < d.Length; i++) r[i] = DateTime.FromOADate(d[i]).Hour; return new FormulaData(r); } }
    protected FormulaData MINUTE { get { var d = DATE; var r = new double[d.Length]; for (int i = 0; i < d.Length; i++) r[i] = DateTime.FromOADate(d[i]).Minute; return new FormulaData(r); } }
    protected FormulaData STOCK => CLOSE;
    protected string STKLABEL => "";
    protected FormulaData ATRI(double n = 14) { var tr = MAX(HIGH - LOW, MAX(ABS(HIGH - REF(CLOSE, 1)), ABS(LOW - REF(CLOSE, 1)))); return MA(tr, n); }

    // SAR as method (different from SAR class in Trend.fml.cs)
    protected FormulaData SAR_CALC(double step = 0.02, double max = 0.2) => SARTURN(HIGH, LOW, CLOSE, step, step, max);

    // Convenience: FormulaData to double conversions
    protected FormulaData SARTURN(FormulaData high, FormulaData low, FormulaData sar, double af, double afStep, double afMax)
    {
        // Parabolic SAR implementation
        var result = new double[high.Length];
        Array.Fill(result, double.NaN);
        if (high.Length < 2) return new FormulaData(result);

        bool isLong = high[1] > high[0];
        double sarVal = isLong ? low[0] : high[0];
        double ep = isLong ? high[0] : low[0];
        double currentAf = af;

        for (int i = 1; i < high.Length; i++)
        {
            sarVal += currentAf * (ep - sarVal);
            if (isLong)
            {
                if (low[i] < sarVal) { isLong = false; sarVal = ep; ep = low[i]; currentAf = af; }
                else { if (high[i] > ep) { ep = high[i]; currentAf = Math.Min(currentAf + afStep, afMax); } }
            }
            else
            {
                if (high[i] > sarVal) { isLong = true; sarVal = ep; ep = high[i]; currentAf = af; }
                else { if (low[i] < ep) { ep = low[i]; currentAf = Math.Min(currentAf + afStep, afMax); } }
            }
            result[i] = sarVal;
        }
        return new FormulaData(result);
    }

    // FML cross-reference (stub — returns empty data for now)
    protected FormulaData FML(IFormulaDataProvider dp, string reference) => new(double.NaN, dp.Count);
    protected FormulaData FML(string reference) => new(double.NaN, Count);
    protected FormulaData GetFormulaData(string formula) => new(double.NaN, Count);

    // ISUP / ISDOWN
    protected FormulaData ISUP
    {
        get
        {
            var c = CLOSE; var o = OPEN;
            var result = new double[c.Length];
            for (int i = 0; i < c.Length; i++) result[i] = c[i] >= o[i] ? 1 : 0;
            return new FormulaData(result);
        }
    }

    protected FormulaData ISDOWN
    {
        get
        {
            var c = CLOSE; var o = OPEN;
            var result = new double[c.Length];
            for (int i = 0; i < c.Length; i++) result[i] = c[i] < o[i] ? 1 : 0;
            return new FormulaData(result);
        }
    }

    // S (alias for CLOSE in some formulas)
    protected FormulaData S => CLOSE;

    // BACKTEST stubs (various overloads)
    protected FormulaData BACKTEST(FormulaData buy, FormulaData sell, FormulaData data) => data;
    protected FormulaData BACKTEST(params object[] args) => new(double.NaN, Count);

    // ========== RENDERING HELPERS ==========

    protected void SETTEXTVISIBLE(FormulaData data, bool visible = false)
    {
        if (!visible) data.SetAttrs("NOTEXT");
    }
    protected void SETTEXTVISIBLE(FormulaData data) => SETTEXTVISIBLE(data, false);

    // ========== ASSEMBLY REGISTRY ==========

    public static void RegAssembly(string key, Assembly assembly)
    {
        _assemblies[key] = assembly;
        _cachedTypes = null;
    }

    public static void UnregAssembly(string key)
    {
        _assemblies.Remove(key);
        _cachedTypes = null;
    }

    public static List<Type> GetAllFormulaTypes()
    {
        if (_cachedTypes != null) return _cachedTypes;
        _cachedTypes = [];
        foreach (var asm in _assemblies.Values)
        {
            try
            {
                foreach (var type in asm.GetTypes())
                    if (type.IsSubclassOf(typeof(FormulaBase)) && !type.IsAbstract)
                        _cachedTypes.Add(type);
            }
            catch { }
        }
        return _cachedTypes;
    }

    public static FormulaBase? CreateByName(string name)
    {
        // Parse "MACD(12,26,9)" format
        var parenIdx = name.IndexOf('(');
        var typeName = parenIdx >= 0 ? name[..parenIdx] : name;
        string[]? paramValues = null;
        if (parenIdx >= 0)
        {
            var paramStr = name[(parenIdx + 1)..].TrimEnd(')');
            paramValues = paramStr.Split(',');
        }

        var type = GetAllFormulaTypes().FirstOrDefault(t =>
            t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        if (type == null) return null;

        var instance = (FormulaBase)Activator.CreateInstance(type)!;

        // Set parameter values
        if (paramValues != null)
        {
            for (int i = 0; i < paramValues.Length && i < instance.Params.Count; i++)
            {
                var field = type.GetField(instance.Params[i].Name);
                if (field != null && field.FieldType == typeof(double))
                    field.SetValue(instance, double.Parse(paramValues[i].Trim()));
            }
        }

        return instance;
    }

    public static string[] GetCategories()
    {
        return GetAllFormulaTypes()
            .Select(t => t.Namespace ?? "FML")
            .Distinct()
            .OrderBy(n => n)
            .ToArray();
    }
}
