namespace ArTraV2.Core.Formula;

public class FormulaData
{
    public double[] Data;
    public string Name { get; set; } = "";
    public FormulaRenderType RenderType { get; set; } = FormulaRenderType.NORMAL;
    public string Attrs { get; set; } = "";

    public FormulaData(double[] data) => Data = data;
    public FormulaData(double value, int count)
    {
        Data = new double[count];
        Array.Fill(Data, value);
    }

    public int Length => Data.Length;
    public double this[int i] { get => Data[i]; set => Data[i] = value; }

    public void SetAttrs(string attrs) => Attrs = attrs;

    // Operator overloads for formula expressions
    public static FormulaData operator +(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x + y);
    public static FormulaData operator -(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x - y);
    public static FormulaData operator *(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x * y);
    public static FormulaData operator /(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => y == 0 ? double.NaN : x / y);
    public static FormulaData operator +(FormulaData a, double b) => UnaryOp(a, x => x + b);
    public static FormulaData operator -(FormulaData a, double b) => UnaryOp(a, x => x - b);
    public static FormulaData operator *(FormulaData a, double b) => UnaryOp(a, x => x * b);
    public static FormulaData operator /(FormulaData a, double b) => UnaryOp(a, x => b == 0 ? double.NaN : x / b);
    public static FormulaData operator +(double a, FormulaData b) => UnaryOp(b, x => a + x);
    public static FormulaData operator -(double a, FormulaData b) => UnaryOp(b, x => a - x);
    public static FormulaData operator *(double a, FormulaData b) => UnaryOp(b, x => a * x);
    public static FormulaData operator /(double a, FormulaData b) => UnaryOp(b, x => x == 0 ? double.NaN : a / x);
    public static FormulaData operator -(FormulaData a) => UnaryOp(a, x => -x);
    public static FormulaData operator >(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x > y ? 1 : 0);
    public static FormulaData operator <(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x < y ? 1 : 0);
    public static FormulaData operator >=(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x >= y ? 1 : 0);
    public static FormulaData operator <=(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x <= y ? 1 : 0);
    public static FormulaData operator >(FormulaData a, double b) => UnaryOp(a, x => x > b ? 1 : 0);
    public static FormulaData operator <(FormulaData a, double b) => UnaryOp(a, x => x < b ? 1 : 0);
    public static FormulaData operator >=(FormulaData a, double b) => UnaryOp(a, x => x >= b ? 1 : 0);
    public static FormulaData operator <=(FormulaData a, double b) => UnaryOp(a, x => x <= b ? 1 : 0);

    public static implicit operator FormulaData(double value) => new([value]);
    public static implicit operator FormulaData(int value) => new([(double)value]);
    public static implicit operator FormulaData(bool value) => new([value ? 1.0 : 0.0]);

    // Logical operators
    public static FormulaData operator &(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => (x != 0 && y != 0) ? 1 : 0);
    public static FormulaData operator |(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => (x != 0 || y != 0) ? 1 : 0);
    public static FormulaData operator &(FormulaData a, bool b) => UnaryOp(a, x => (x != 0 && b) ? 1 : 0);
    public static FormulaData operator |(FormulaData a, bool b) => UnaryOp(a, x => (x != 0 || b) ? 1 : 0);
    public static FormulaData operator ==(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x == y ? 1 : 0);
    public static FormulaData operator !=(FormulaData a, FormulaData b) => BinaryOp(a, b, (x, y) => x != y ? 1 : 0);
    public static FormulaData operator ==(FormulaData a, int b) => UnaryOp(a, x => x == b ? 1 : 0);
    public static FormulaData operator !=(FormulaData a, int b) => UnaryOp(a, x => x != b ? 1 : 0);
    public static FormulaData operator ==(FormulaData a, double b) => UnaryOp(a, x => x == b ? 1 : 0);
    public static FormulaData operator !=(FormulaData a, double b) => UnaryOp(a, x => x != b ? 1 : 0);
    public static bool operator true(FormulaData a) => a.Data.Length > 0 && a.Data[^1] != 0;
    public static bool operator false(FormulaData a) => a.Data.Length == 0 || a.Data[^1] == 0;

    public double LASTDATA => Data.Length > 0 ? Data[^1] : double.NaN;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);
    public override int GetHashCode() => base.GetHashCode();

    public static FormulaData BinaryOp2(FormulaData a, FormulaData b, Func<double, double, double> op) => BinaryOp(a, b, op);
    public static FormulaData UnaryOp2(FormulaData a, Func<double, double> op) => UnaryOp(a, op);

    private static FormulaData BinaryOp(FormulaData a, FormulaData b, Func<double, double, double> op)
    {
        int len = Math.Max(a.Length, b.Length);
        var result = new double[len];
        for (int i = 0; i < len; i++)
        {
            var va = i < a.Length ? a[i] : double.NaN;
            var vb = i < b.Length ? b[i] : double.NaN;
            result[i] = (double.IsNaN(va) || double.IsNaN(vb)) ? double.NaN : op(va, vb);
        }
        return new FormulaData(result);
    }

    private static FormulaData UnaryOp(FormulaData a, Func<double, double> op)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = double.IsNaN(a[i]) ? double.NaN : op(a[i]);
        return new FormulaData(result);
    }
}

public class FormulaPackage
{
    public FormulaData[] DataArray { get; }
    public string Info { get; }

    public FormulaPackage(FormulaData[] data, string info)
    {
        DataArray = data;
        Info = info;
    }
}

public enum FormulaRenderType
{
    NORMAL, LINE, COLORSTICK, VOLSTICK, STICKLINE, STOCK, CANDLE,
    ICON, TEXT, POLY, FILLRGN, FILLAREA, PARTLINE, VERTLINE, AXISY
}

public enum FormulaParamType
{
    Double,
    String,
    Symbol,
    Indicator
}

public class FormulaParam
{
    public string Name { get; set; } = "";
    public string DefaultValue { get; set; } = "0";
    public string MinValue { get; set; } = "0";
    public string MaxValue { get; set; } = "0";
    public string Step { get; set; } = "1";
    public string Description { get; set; } = "";
    public FormulaParamType ParamType { get; set; } = FormulaParamType.Double;
}
