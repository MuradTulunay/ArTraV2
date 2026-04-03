using ArTraV2.Core.Models;

namespace ArTraV2.Core.Interfaces;

public interface ILiveDataProvider : IDataProvider
{
    event Action<BarData>? OnLiveBar;
    Task ConnectAsync(string symbol, string interval, CancellationToken ct = default);
    void Disconnect();
    bool IsConnected { get; }
}
