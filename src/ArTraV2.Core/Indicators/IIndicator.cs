using ArTraV2.Core.Models;

namespace ArTraV2.Core.Indicators;

public interface IIndicator
{
    string Name { get; }
    string ShortName { get; }
    bool IsOverlay { get; }
    IndicatorParameter[] Parameters { get; }
    double[] ReferenceLines { get; }
    List<IndicatorResult> Calculate(List<BarData> data);
}
