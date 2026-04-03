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
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _wsCts;
    private string? _subscribedSymbol;

    public string BaseUrl { get; set; } = "http://178.104.110.229:8081";
    public string WsUrl => BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://") + "/ws/bars";
    public string Name => "Murad Bot";
    public DataSource Source => DataSource.Binance;
    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public event Action<BarData>? OnLiveBar;

    public MuradBotProvider()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.Add("User-Agent", "ArTraV2/1.0");
    }

    public MuradBotProvider(string baseUrl) : this()
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }

    // --- Historical Data: GET /api/rt/bars?symbol=GARAN&tf=15m&n=500 ---

    public async Task<List<BarData>> GetHistoricalDataAsync(
        string symbol, DataCycle cycle, DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var tf = CycleToTf(cycle);
        var botSymbol = NormalizeSymbolForBot(symbol);

        // Calculate approximate bar count from date range
        var barCount = EstimateBarCount(cycle, startDate, endDate);
        barCount = Math.Min(barCount, 30000);

        // Fetch in chunks of 500 (API max)
        var allBars = new List<BarData>();
        var remaining = barCount;

        // First request — get latest N bars
        var n = Math.Min(remaining, 500);
        var url = $"{BaseUrl}/api/rt/bars?symbol={Uri.EscapeDataString(botSymbol)}&tf={tf}&n={n}";
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = ParseBarsResponse(json);
        allBars.AddRange(result.Bars);

        // If forming bar exists, add it as the last bar
        if (result.FormingBar != null)
            allBars.Add(result.FormingBar);

        return allBars;
    }

    // --- Symbol Search: GET /api/v2/symbols ---

    public async Task<List<string>> SearchSymbolsAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/api/v2/symbols", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var results = new List<string>();
            var upperQuery = query.ToUpperInvariant();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                string? symbol = null;

                // Support both flat string array and object array
                if (item.ValueKind == JsonValueKind.String)
                {
                    symbol = item.GetString();
                }
                else if (item.TryGetProperty("symbol", out var symProp))
                {
                    symbol = symProp.GetString();
                }

                if (symbol != null && symbol.Contains(upperQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(symbol);
                    if (results.Count >= 30) break;
                }
            }

            return results;
        }
        catch
        {
            return [];
        }
    }

    public async Task<BarData?> GetLatestBarAsync(string symbol, CancellationToken ct = default)
    {
        var tf = "1D";
        var botSymbol = NormalizeSymbolForBot(symbol);
        var url = $"{BaseUrl}/api/rt/bars?symbol={Uri.EscapeDataString(botSymbol)}&tf={tf}&n=1";

        try
        {
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = ParseBarsResponse(json);

            return result.FormingBar ?? result.Bars.LastOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // --- ILiveDataProvider: ws://host:port/ws/bars ---

    public Task ConnectAsync(string symbol, string interval, CancellationToken ct = default)
    {
        _subscribedSymbol = NormalizeSymbolForBot(symbol).ToUpperInvariant();
        return ConnectWebSocketAsync(ct);
    }

    public void Disconnect() => DisconnectWebSocket();

    private async Task ConnectWebSocketAsync(CancellationToken ct)
    {
        DisconnectWebSocket();
        _wsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(new Uri(WsUrl), _wsCts.Token);
        _ = Task.Run(() => ReceiveLoopAsync(_wsCts.Token), _wsCts.Token);
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

                    // murad-bot bar format:
                    // {"type":"bar","symbol":"GARAN","exchange":"BIST","interval":"15m",
                    //  "data":{"t":1775226600,"o":131.0,"h":131.3,"l":130.8,"c":130.9,"v":1020262.0}}
                    if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "bar")
                    {
                        var msgSymbol = root.GetProperty("symbol").GetString()?.ToUpperInvariant();

                        // Filter: only pass bars for the subscribed symbol
                        if (_subscribedSymbol != null && msgSymbol != null &&
                            !SymbolMatches(msgSymbol, _subscribedSymbol))
                        {
                            continue;
                        }

                        var data = root.GetProperty("data");
                        var timestamp = data.GetProperty("t");

                        // Timestamp can be seconds or milliseconds
                        var tsValue = timestamp.GetInt64();
                        var date = tsValue > 9_999_999_999
                            ? DateTimeOffset.FromUnixTimeMilliseconds(tsValue).UtcDateTime
                            : DateTimeOffset.FromUnixTimeSeconds(tsValue).UtcDateTime;

                        var bar = new BarData
                        {
                            Date = date,
                            Open = data.GetProperty("o").GetDouble(),
                            High = data.GetProperty("h").GetDouble(),
                            Low = data.GetProperty("l").GetDouble(),
                            Close = data.GetProperty("c").GetDouble(),
                            Volume = data.GetProperty("v").GetDouble()
                        };
                        OnLiveBar?.Invoke(bar);
                    }
                    // Also handle raw array format: [timestamp, o, h, l, c, v]
                    // with symbol in a wrapper
                    else if (root.TryGetProperty("symbol", out var symProp) &&
                             root.TryGetProperty("bar", out var barArr) &&
                             barArr.ValueKind == JsonValueKind.Array)
                    {
                        var msgSymbol = symProp.GetString()?.ToUpperInvariant();
                        if (_subscribedSymbol != null && msgSymbol != null &&
                            !SymbolMatches(msgSymbol, _subscribedSymbol))
                        {
                            continue;
                        }

                        var bar = ParseBarArray(barArr);
                        if (bar != null)
                            OnLiveBar?.Invoke(bar);
                    }
                }
                catch { /* Skip malformed messages */ }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
    }

    private void DisconnectWebSocket()
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

    // --- Bot Status ---

    public async Task<MuradBotStatus?> GetBotStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/api/status", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<MuradBotStatus>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch { return null; }
    }

    // --- Parsers ---

    private record BarsResult(List<BarData> Bars, BarData? FormingBar);

    private static BarsResult ParseBarsResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var bars = new List<BarData>();

        // Parse "bars": [[timestamp, o, h, l, c, v], ...]
        if (root.TryGetProperty("bars", out var barsArr))
        {
            foreach (var barArr in barsArr.EnumerateArray())
            {
                var bar = ParseBarArray(barArr);
                if (bar != null)
                    bars.Add(bar);
            }
        }

        // Parse "forming": [timestamp, o, h, l, c, v] or null
        BarData? formingBar = null;
        if (root.TryGetProperty("forming", out var forming) &&
            forming.ValueKind == JsonValueKind.Array)
        {
            formingBar = ParseBarArray(forming);
        }

        return new BarsResult(bars, formingBar);
    }

    private static BarData? ParseBarArray(JsonElement arr)
    {
        if (arr.GetArrayLength() < 6) return null;

        var tsValue = arr[0].GetInt64();
        var date = tsValue > 9_999_999_999
            ? DateTimeOffset.FromUnixTimeMilliseconds(tsValue).UtcDateTime
            : DateTimeOffset.FromUnixTimeSeconds(tsValue).UtcDateTime;

        return new BarData
        {
            Date = date,
            Open = arr[1].GetDouble(),
            High = arr[2].GetDouble(),
            Low = arr[3].GetDouble(),
            Close = arr[4].GetDouble(),
            Volume = arr[5].GetDouble()
        };
    }

    // --- Helpers ---

    private static bool SymbolMatches(string msgSymbol, string subscribed)
    {
        if (msgSymbol.Equals(subscribed, StringComparison.OrdinalIgnoreCase))
            return true;
        // GARAN matches GARAN.IS and vice versa
        var clean1 = msgSymbol.Replace(".IS", "").Replace(".E", "");
        var clean2 = subscribed.Replace(".IS", "").Replace(".E", "");
        return clean1.Equals(clean2, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSymbolForBot(string symbol)
    {
        // Bot uses GARAN (no .IS suffix) for BIST
        var upper = symbol.ToUpperInvariant();
        if (upper.EndsWith(".IS")) return upper[..^3];
        if (upper.EndsWith(".E")) return upper[..^2];
        return upper;
    }

    private static string CycleToTf(DataCycle cycle) => cycle.CycleBase switch
    {
        DataCycleBase.Minute => $"{cycle.Multiplier}m",
        DataCycleBase.Hour => $"{cycle.Multiplier}h",
        DataCycleBase.Day => "1D",
        DataCycleBase.Week => "1W",
        DataCycleBase.Month => "1M",
        _ => "1D"
    };

    private static int EstimateBarCount(DataCycle cycle, DateTime start, DateTime end)
    {
        var span = end - start;
        return cycle.CycleBase switch
        {
            DataCycleBase.Minute => (int)(span.TotalMinutes / cycle.Multiplier),
            DataCycleBase.Hour => (int)(span.TotalHours / cycle.Multiplier),
            DataCycleBase.Day => (int)(span.TotalDays / cycle.Multiplier),
            DataCycleBase.Week => (int)(span.TotalDays / 7 / cycle.Multiplier),
            DataCycleBase.Month => (int)(span.TotalDays / 30 / cycle.Multiplier),
            _ => 500
        };
    }

    public void Dispose()
    {
        DisconnectWebSocket();
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
