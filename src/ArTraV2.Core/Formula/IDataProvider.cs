namespace ArTraV2.Core.Formula;

/// <summary>
/// EFC-compatible data provider interface for formula execution
/// </summary>
public interface IFormulaDataProvider
{
    double[] this[string name] { get; }
    int Count { get; }
}

/// <summary>
/// Adapter from ArTraV2 BarData list to formula data provider
/// </summary>
public class BarDataProvider : IFormulaDataProvider
{
    private readonly Dictionary<string, double[]> _data = new(StringComparer.OrdinalIgnoreCase);
    public int Count { get; }

    public BarDataProvider(List<Models.BarData> bars)
    {
        Count = bars.Count;
        var open = new double[Count];
        var high = new double[Count];
        var low = new double[Count];
        var close = new double[Count];
        var volume = new double[Count];
        var date = new double[Count];

        for (int i = 0; i < Count; i++)
        {
            open[i] = bars[i].Open;
            high[i] = bars[i].High;
            low[i] = bars[i].Low;
            close[i] = bars[i].Close;
            volume[i] = bars[i].Volume;
            date[i] = bars[i].Date.ToOADate();
        }

        _data["OPEN"] = open;
        _data["HIGH"] = high;
        _data["LOW"] = low;
        _data["CLOSE"] = close;
        _data["VOLUME"] = volume;
        _data["DATE"] = date;
        _data["O"] = open;
        _data["H"] = high;
        _data["L"] = low;
        _data["C"] = close;
        _data["V"] = volume;
        _data["VOL"] = volume;
    }

    public double[] this[string name] =>
        _data.TryGetValue(name, out var arr) ? arr : new double[Count];
}
