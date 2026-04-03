using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.DataProviders;

public class MuradBotProvider : ILiveDataProvider, IDisposable
{
    private readonly HttpClient _http;
    private readonly YahooFinanceProvider _yahoo = new();
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _wsCts;

    public string DashboardUrl { get; set; } = "http://localhost:8080";
    public string WsUrl { get; set; } = "ws://localhost:8080/ws";
    public string Name => "Murad Bot";
    public DataSource Source => DataSource.Binance;
    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public event Action<BarData>? OnLiveBar;

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

    // --- Symbol Detection ---

    private static bool IsCryptoSymbol(string symbol)
    {
        var upper = symbol.ToUpperInvariant();
        return upper.EndsWith("USDT") || upper.EndsWith("BTC") || upper.EndsWith("ETH")
            || upper.EndsWith("BUSD") || upper.EndsWith("BNB") || upper.EndsWith("USDC")
            || upper.EndsWith("FDUSD") || upper.EndsWith("TRY");
    }

    private static bool IsBistSymbol(string symbol)
    {
        // Explicit .IS suffix or pure alphabetic (Turkish stock ticker)
        if (symbol.EndsWith(".IS", StringComparison.OrdinalIgnoreCase)) return true;
        if (symbol.EndsWith(".E", StringComparison.OrdinalIgnoreCase)) return true;
        // Not crypto → assume BIST
        return !IsCryptoSymbol(symbol);
    }

    private static string NormalizeBistSymbol(string symbol)
    {
        // Add .IS suffix if not present for Yahoo Finance
        if (symbol.EndsWith(".IS", StringComparison.OrdinalIgnoreCase))
            return symbol.ToUpper();
        return symbol.ToUpper() + ".IS";
    }

    // --- IDataProvider ---

    public async Task<List<BarData>> GetHistoricalDataAsync(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        if (IsBistSymbol(symbol))
        {
            // BIST → Yahoo Finance
            var yahooSymbol = NormalizeBistSymbol(symbol);
            return await _yahoo.GetHistoricalDataAsync(yahooSymbol, cycle, startDate, endDate, ct);
        }

        // Crypto → try murad-bot dashboard first, then Binance direct
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

        return await FetchBinancePaginated(symbol, cycle, startDate, endDate, 30000, ct);
    }

    public async Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default)
    {
        var results = new List<string>();

        // Search BIST via Yahoo
        try
        {
            var yahooResults = await _yahoo.SearchSymbolsAsync(query + ".IS", ct);
            results.AddRange(yahooResults.Where(s => s.EndsWith(".IS")).Take(10));
        }
        catch { }

        // Search Binance
        try
        {
            var response = await _http.GetAsync("https://api.binance.com/api/v3/exchangeInfo", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
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
            }
        }
        catch { }

        return results;
    }

    public async Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default)
    {
        if (IsBistSymbol(symbol))
        {
            var yahooSymbol = NormalizeBistSymbol(symbol);
            return await _yahoo.GetLatestBarAsync(yahooSymbol, ct);
        }

        // Crypto → try bot status first
        try
        {
            var response = await _http.GetAsync($"{DashboardUrl}/api/status", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
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

        var url = $"https://api.binance.com/api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval=1m&limit=1";
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        var klines = ParseBinanceKlines(await resp.Content.ReadAsStringAsync(ct));
        return klines.LastOrDefault();
    }

    // --- ILiveDataProvider ---

    private string? _subscribedSymbol;

    public Task ConnectAsync(string symbol, string interval, CancellationToken ct = default)
    {
        _subscribedSymbol = symbol.ToUpperInvariant();
        return ConnectLiveStreamAsync(symbol, interval, ct);
    }

    public void Disconnect() => DisconnectLiveStream();

    // --- WebSocket Live Stream (BIST + Binance via murad-bot) ---

    public async Task ConnectLiveStreamAsync(string symbol = "ethusdt", string interval = "1m", CancellationToken ct = default)
    {
        DisconnectLiveStream();
        _wsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Try murad-bot dashboard WebSocket first (supports both BIST + Binance)
        try
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(WsUrl), _wsCts.Token);
            _ = Task.Run(() => ReceiveLoopAsync(_wsCts.Token), _wsCts.Token);
            return;
        }
        catch
        {
            _ws?.Dispose();
        }

        // Fallback: Binance WebSocket (crypto only)
        if (IsCryptoSymbol(symbol))
        {
            _ws = new ClientWebSocket();
            var binanceWs = $"wss://stream.binance.com:9443/ws/{symbol.ToLower()}@kline_{interval}";
            await _ws.ConnectAsync(new Uri(binanceWs), _wsCts.Token);
            _ = Task.Run(() => ReceiveLoopAsync(_wsCts.Token), _wsCts.Token);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();
        try
        {
            while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                sb.Clear();
                WebSocketReceiveResult result;
                do
                {
                    result = await _ws.ReceiveAsync(buffer, ct);
                    if (result.MessageType == WebSocketMessageType.Close) return;
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage);

                try
                {
                    using var doc = JsonDocument.Parse(sb.ToString());
                    var root = doc.RootElement;

                    // murad-bot unified bar format:
                    // {"type":"bar","symbol":"THYAO.IS","exchange":"BIST","interval":"1d","data":{"t":...,"o":...,"h":...,"l":...,"c":...,"v":...}}
                    if (root.TryGetProperty("type", out var type) && type.GetString() == "bar")
                    {
                        var msgSymbol = root.GetProperty("symbol").GetString()?.ToUpperInvariant();

                        // Filter: only pass bars for the subscribed symbol
                        if (_subscribedSymbol != null && msgSymbol != null &&
                            !msgSymbol.Equals(_subscribedSymbol, StringComparison.OrdinalIgnoreCase) &&
                            !msgSymbol.Replace(".IS", "").Equals(_subscribedSymbol.Replace(".IS", ""), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var data = root.GetProperty("data");
                        var bar = new BarData
                        {
                            Date = DateTimeOffset.FromUnixTimeMilliseconds(data.GetProperty("t").GetInt64()).UtcDateTime,
                            Open = data.GetProperty("o").GetDouble(),
                            High = data.GetProperty("h").GetDouble(),
                            Low = data.GetProperty("l").GetDouble(),
                            Close = data.GetProperty("c").GetDouble(),
                            Volume = data.GetProperty("v").GetDouble()
                        };
                        OnLiveBar?.Invoke(bar);
                    }
                    // Binance native kline format (fallback WS)
                    else if (root.TryGetProperty("k", out var kline))
                    {
                        var bar = new BarData
                        {
                            Date = DateTimeOffset.FromUnixTimeMilliseconds(kline.GetProperty("t").GetInt64()).UtcDateTime,
                            Open = double.Parse(kline.GetProperty("o").GetString()!, CultureInfo.InvariantCulture),
                            High = double.Parse(kline.GetProperty("h").GetString()!, CultureInfo.InvariantCulture),
                            Low = double.Parse(kline.GetProperty("l").GetString()!, CultureInfo.InvariantCulture),
                            Close = double.Parse(kline.GetProperty("c").GetString()!, CultureInfo.InvariantCulture),
                            Volume = double.Parse(kline.GetProperty("v").GetString()!, CultureInfo.InvariantCulture)
                        };
                        OnLiveBar?.Invoke(bar);
                    }
                }
                catch { /* Skip malformed messages */ }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
    }

    // --- Bot Status ---

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
        if (_ws is { State: WebSocketState.Open })
        {
            try { _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait(2000); }
            catch { }
        }
        _ws?.Dispose();
        _ws = null;
        _wsCts?.Dispose();
        _wsCts = null;
    }

    // --- Binance Paginated Fetch ---

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

            await Task.Delay(100, ct);
        }

        return allBars;
    }

    // --- Parsers ---

    private static List<BarData> ParseDashboardKlines(string json)
    {
        var bars = new List<BarData>();
        using var doc = JsonDocument.Parse(json);

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            bars.Add(new BarData
            {
                Date = item.TryGetProperty("timestamp", out var ts)
                    ? DateTime.Parse(ts.GetString()!, CultureInfo.InvariantCulture)
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
                Open = double.Parse(kline[1].GetString()!, CultureInfo.InvariantCulture),
                High = double.Parse(kline[2].GetString()!, CultureInfo.InvariantCulture),
                Low = double.Parse(kline[3].GetString()!, CultureInfo.InvariantCulture),
                Close = double.Parse(kline[4].GetString()!, CultureInfo.InvariantCulture),
                Volume = double.Parse(kline[5].GetString()!, CultureInfo.InvariantCulture),
                AdjClose = double.Parse(kline[4].GetString()!, CultureInfo.InvariantCulture)
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
        _yahoo.Dispose();
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
