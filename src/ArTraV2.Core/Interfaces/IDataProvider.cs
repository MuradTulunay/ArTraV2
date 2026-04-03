using ArTraV2.Core.Models;

namespace ArTraV2.Core.Interfaces;

public interface IDataProvider
{
    string Name { get; }
    DataSource Source { get; }
    Task<List<BarData>> GetHistoricalDataAsync(string symbol, DataCycle cycle, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default);
    Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default);
}
