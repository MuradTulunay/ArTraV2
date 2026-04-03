using System.Globalization;
using System.Text.Json;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.DataProviders;

public class YahooFinanceProvider : IDataProvider, IDisposable
{
    private readonly HttpClient _http;

    public string Name => "Yahoo Finance";
    public DataSource Source => DataSource.YahooFinance;

    public YahooFinanceProvider()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "ArTraV2/1.0");
    }

    public async Task<List<BarData>> GetHistoricalDataAsync(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var interval = CycleToInterval(cycle);
        var period1 = new DateTimeOffset(startDate).ToUnixTimeSeconds();
        var period2 = new DateTimeOffset(endDate).ToUnixTimeSeconds();

        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}" +
                  $"?period1={period1}&period2={period2}&interval={interval}&includeAdjustedClose=true";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return ParseChartResponse(json);
    }

    public async Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default)
    {
        var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={Uri.EscapeDataString(query)}&quotesCount=10&newsCount=0";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var results = new List<string>();
        if (doc.RootElement.TryGetProperty("quotes", out var quotes))
        {
            foreach (var quote in quotes.EnumerateArray())
            {
                if (quote.TryGetProperty("symbol", out var sym))
                    results.Add(sym.GetString()!);
            }
        }
        return results;
    }

    public async Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default)
    {
        var bars = await GetHistoricalDataAsync(symbol, DataCycle.Daily,
            DateTime.UtcNow.AddDays(-5), DateTime.UtcNow, ct);
        return bars.LastOrDefault();
    }

    private static List<BarData> ParseChartResponse(string json)
    {
        var bars = new List<BarData>();
        using var doc = JsonDocument.Parse(json);

        var result = doc.RootElement
            .GetProperty("chart")
            .GetProperty("result")[0];

        var timestamps = result.GetProperty("timestamp");
        var indicators = result.GetProperty("indicators");
        var quote = indicators.GetProperty("quote")[0];

        var opens = quote.GetProperty("open");
        var highs = quote.GetProperty("high");
        var lows = quote.GetProperty("low");
        var closes = quote.GetProperty("close");
        var volumes = quote.GetProperty("volume");

        JsonElement? adjCloses = null;
        if (indicators.TryGetProperty("adjclose", out var adjCloseArr) && adjCloseArr.GetArrayLength() > 0)
            adjCloses = adjCloseArr[0].GetProperty("adjclose");

        for (int i = 0; i < timestamps.GetArrayLength(); i++)
        {
            var open = GetDouble(opens, i);
            var high = GetDouble(highs, i);
            var low = GetDouble(lows, i);
            var close = GetDouble(closes, i);

            if (open == 0 && high == 0 && low == 0 && close == 0) continue;

            bars.Add(new BarData
            {
                Date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).UtcDateTime,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = GetDouble(volumes, i),
                AdjClose = adjCloses.HasValue ? GetDouble(adjCloses.Value, i) : close
            });
        }

        return bars;
    }

    private static double GetDouble(JsonElement arr, int index)
    {
        var el = arr[index];
        if (el.ValueKind == JsonValueKind.Null) return 0;
        return el.GetDouble();
    }

    private static string CycleToInterval(DataCycle cycle) => cycle.CycleBase switch
    {
        DataCycleBase.Minute => cycle.Multiplier switch
        {
            1 => "1m",
            2 => "2m",
            5 => "5m",
            15 => "15m",
            30 => "30m",
            60 => "60m",
            90 => "90m",
            _ => $"{cycle.Multiplier}m"
        },
        DataCycleBase.Hour => cycle.Multiplier switch
        {
            1 => "1h",
            _ => $"{cycle.Multiplier}h"
        },
        DataCycleBase.Day => "1d",
        DataCycleBase.Week => "1wk",
        DataCycleBase.Month => "1mo",
        DataCycleBase.Quarter => "3mo",
        _ => "1d"
    };

    public void Dispose() => _http.Dispose();
}
