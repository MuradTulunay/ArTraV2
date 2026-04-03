using System.Text.Json;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.DataProviders;

public class BinanceProvider : IDataProvider, IDisposable
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://api.binance.com";

    public string Name => "Binance";
    public DataSource Source => DataSource.Binance;

    public BinanceProvider()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Add("User-Agent", "ArTraV2/1.0");
    }

    public async Task<List<BarData>> GetHistoricalDataAsync(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var interval = CycleToInterval(cycle);
        var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
        var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

        var allBars = new List<BarData>();
        var currentStart = startMs;

        while (currentStart < endMs)
        {
            var url = $"/api/v3/klines?symbol={Uri.EscapeDataString(symbol)}" +
                      $"&interval={interval}&startTime={currentStart}&endTime={endMs}&limit=1000";

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var batch = ParseKlines(json);

            if (batch.Count == 0) break;

            allBars.AddRange(batch);
            currentStart = new DateTimeOffset(batch[^1].Date).ToUnixTimeMilliseconds() + 1;

            if (batch.Count < 1000) break;
        }

        return allBars;
    }

    public async Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default)
    {
        var response = await _http.GetAsync("/api/v3/exchangeInfo", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var results = new List<string>();
        var upperQuery = query.ToUpperInvariant();

        foreach (var sym in doc.RootElement.GetProperty("symbols").EnumerateArray())
        {
            var symbol = sym.GetProperty("symbol").GetString()!;
            var status = sym.GetProperty("status").GetString();
            if (status == "TRADING" && symbol.Contains(upperQuery, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(symbol);
                if (results.Count >= 20) break;
            }
        }

        return results;
    }

    public async Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default)
    {
        var url = $"/api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval=1d&limit=1";
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var bars = ParseKlines(json);
        return bars.LastOrDefault();
    }

    private static List<BarData> ParseKlines(string json)
    {
        var bars = new List<BarData>();
        using var doc = JsonDocument.Parse(json);

        foreach (var kline in doc.RootElement.EnumerateArray())
        {
            var openTime = kline[0].GetInt64();
            bars.Add(new BarData
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds(openTime).UtcDateTime,
                Open = double.Parse(kline[1].GetString()!),
                High = double.Parse(kline[2].GetString()!),
                Low = double.Parse(kline[3].GetString()!),
                Close = double.Parse(kline[4].GetString()!),
                Volume = double.Parse(kline[5].GetString()!),
                AdjClose = double.Parse(kline[4].GetString()!)
            });
        }

        return bars;
    }

    private static string CycleToInterval(DataCycle cycle) => cycle.CycleBase switch
    {
        DataCycleBase.Second => $"{cycle.Multiplier}s",
        DataCycleBase.Minute => $"{cycle.Multiplier}m",
        DataCycleBase.Hour => $"{cycle.Multiplier}h",
        DataCycleBase.Day => $"{cycle.Multiplier}d",
        DataCycleBase.Week => $"{cycle.Multiplier}w",
        DataCycleBase.Month => $"{cycle.Multiplier}M",
        _ => "1d"
    };

    public void Dispose() => _http.Dispose();
}
