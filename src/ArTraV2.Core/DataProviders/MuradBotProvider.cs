using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.DataProviders;

public class MuradBotProvider : IDataProvider, IDisposable
{
    private readonly HttpClient _http;
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _wsCts;

    public string DashboardUrl { get; set; } = "http://localhost:8080";
    public string WsUrl { get; set; } = "ws://localhost:8080/ws";
    public string Name => "Murad Bot";
    public DataSource Source => DataSource.Binance;

    public event Action<BarData>? OnLiveBar;
    public event Action<MuradBotStatus>? OnStatusUpdate { add { } remove { } }

    public MuradBotProvider()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "ArTraV2/1.0");
    }

    public MuradBotProvider(string dashboardUrl) : this()
    {
        DashboardUrl = dashboardUrl;
        WsUrl = dashboardUrl.Replace("http://", "ws://").Replace("https://", "wss://") + "/ws";
    }

    public async Task<List<BarData>> GetHistoricalDataAsync(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        // Try murad-bot dashboard first for cached data
        try
        {
            var interval = CycleToInterval(cycle);
            var url = $"{DashboardUrl}/api/klines?symbol={symbol}&interval={interval}&limit=30000";
            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                return ParseDashboardKlines(json);
            }
        }
        catch { /* Fall through to direct Binance */ }

        // Fallback: fetch directly from Binance with pagination for up to 30000 bars
        return await FetchBinancePaginated(symbol, cycle, startDate, endDate, 30000, ct);
    }

    private async Task<List<BarData>> FetchBinancePaginated(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        int maxBars, CancellationToken ct)
    {
        var interval = CycleToInterval(cycle);
        var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
        var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

        var allBars = new List<BarData>();
        var currentStart = startMs;

        while (currentStart < endMs && allBars.Count < maxBars)
        {
            var limit = Math.Min(1000, maxBars - allBars.Count);
            var url = $"https://api.binance.com/api/v3/klines?symbol={Uri.EscapeDataString(symbol)}" +
                      $"&interval={interval}&startTime={currentStart}&endTime={endMs}&limit={limit}";

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var batch = ParseBinanceKlines(json);

            if (batch.Count == 0) break;

            allBars.AddRange(batch);
            currentStart = new DateTimeOffset(batch[^1].Date).ToUnixTimeMilliseconds() + 1;

            if (batch.Count < limit) break;

            // Rate limit respect
            await Task.Delay(100, ct);
        }

        return allBars;
    }

    public async Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default)
    {
        // murad-bot primarily trades ETHUSDT, but support search via Binance
        var response = await _http.GetAsync("https://api.binance.com/api/v3/exchangeInfo", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var results = new List<string>();
        var upperQuery = query.ToUpperInvariant();

        foreach (var sym in doc.RootElement.GetProperty("symbols").EnumerateArray())
        {
            var symbol = sym.GetProperty("symbol").GetString()!;
            if (sym.GetProperty("status").GetString() == "TRADING" &&
                symbol.Contains(upperQuery, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(symbol);
                if (results.Count >= 20) break;
            }
        }
        return results;
    }

    public async Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{DashboardUrl}/api/status", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Parse latest price from bot status
                if (doc.RootElement.TryGetProperty("last_price", out var price))
                {
                    return new BarData
                    {
                        Date = DateTime.UtcNow,
                        Close = price.GetDouble(),
                        Open = price.GetDouble(),
                        High = price.GetDouble(),
                        Low = price.GetDouble()
                    };
                }
            }
        }
        catch { }

        // Fallback to Binance
        var url = $"https://api.binance.com/api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval=1m&limit=1";
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        var klines = ParseBinanceKlines(await resp.Content.ReadAsStringAsync(ct));
        return klines.LastOrDefault();
    }

    // WebSocket live stream
    public async Task ConnectLiveStreamAsync(string symbol = "ethusdt", CancellationToken ct = default)
    {
        _wsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Try murad-bot dashboard WebSocket first
        try
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(WsUrl), _wsCts.Token);
            _ = ReceiveLoopAsync(_wsCts.Token);
            return;
        }
        catch
        {
            _ws?.Dispose();
        }

        // Fallback: connect to Binance WebSocket directly
        _ws = new ClientWebSocket();
        var binanceWs = $"wss://stream.binance.com:9443/ws/{symbol.ToLower()}@kline_1m";
        await _ws.ConnectAsync(new Uri(binanceWs), _wsCts.Token);
        _ = ReceiveLoopAsync(_wsCts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        try
        {
            while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Binance kline format
                    if (root.TryGetProperty("k", out var kline))
                    {
                        var bar = new BarData
                        {
                            Date = DateTimeOffset.FromUnixTimeMilliseconds(kline.GetProperty("t").GetInt64()).UtcDateTime,
                            Open = double.Parse(kline.GetProperty("o").GetString()!),
                            High = double.Parse(kline.GetProperty("h").GetString()!),
                            Low = double.Parse(kline.GetProperty("l").GetString()!),
                            Close = double.Parse(kline.GetProperty("c").GetString()!),
                            Volume = double.Parse(kline.GetProperty("v").GetString()!)
                        };
                        OnLiveBar?.Invoke(bar);
                    }
                    // murad-bot dashboard format
                    else if (root.TryGetProperty("type", out var type))
                    {
                        if (type.GetString() == "ticker" && root.TryGetProperty("data", out var data))
                        {
                            var bar = new BarData
                            {
                                Date = DateTime.UtcNow,
                                Close = data.GetProperty("c").GetDouble(),
                                Open = data.GetProperty("o").GetDouble(),
                                High = data.GetProperty("h").GetDouble(),
                                Low = data.GetProperty("l").GetDouble(),
                                Volume = data.GetProperty("v").GetDouble()
                            };
                            OnLiveBar?.Invoke(bar);
                        }
                    }
                }
                catch { /* Skip malformed messages */ }
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task<MuradBotStatus?> GetBotStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{DashboardUrl}/api/status", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<MuradBotStatus>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch { return null; }
    }

    public void DisconnectLiveStream()
    {
        _wsCts?.Cancel();
        _ws?.Dispose();
        _ws = null;
    }

    private static List<BarData> ParseDashboardKlines(string json)
    {
        var bars = new List<BarData>();
        using var doc = JsonDocument.Parse(json);

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            bars.Add(new BarData
            {
                Date = item.TryGetProperty("timestamp", out var ts)
                    ? DateTime.Parse(ts.GetString()!)
                    : DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("t").GetInt64()).UtcDateTime,
                Open = GetNum(item, "open", "o"),
                High = GetNum(item, "high", "h"),
                Low = GetNum(item, "low", "l"),
                Close = GetNum(item, "close", "c"),
                Volume = GetNum(item, "volume", "v")
            });
        }
        return bars;
    }

    private static double GetNum(JsonElement el, string key1, string key2)
    {
        if (el.TryGetProperty(key1, out var v)) return v.GetDouble();
        if (el.TryGetProperty(key2, out v)) return v.GetDouble();
        return 0;
    }

    private static List<BarData> ParseBinanceKlines(string json)
    {
        var bars = new List<BarData>();
        using var doc = JsonDocument.Parse(json);

        foreach (var kline in doc.RootElement.EnumerateArray())
        {
            bars.Add(new BarData
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds(kline[0].GetInt64()).UtcDateTime,
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

    public void Dispose()
    {
        DisconnectLiveStream();
        _wsCts?.Dispose();
        _http.Dispose();
    }
}

public class MuradBotStatus
{
    public string? Symbol { get; set; }
    public string? Position { get; set; }
    public double? LastPrice { get; set; }
    public double? EntryPrice { get; set; }
    public double? PnL { get; set; }
    public double? Balance { get; set; }
    public string? Signal { get; set; }
    public bool? DryRun { get; set; }
    public string? Mode { get; set; }
}
