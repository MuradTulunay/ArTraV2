using ArTraV2.Core.Chart;
using ArTraV2.Core.DataProviders;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;

namespace ArTraV2.App;

public partial class MainForm : Form
{
    private readonly ChartRenderer _chart = new();
    private readonly Panel _chartPanel = new();
    private readonly ComboBox _cmbDataSource = new();
    private readonly TextBox _txtSymbol = new();
    private readonly Button _btnLoad = new();
    private readonly ComboBox _cmbCycle = new();
    private readonly ComboBox _cmbRenderType = new();
    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _lblStatus = new();
    private readonly ToolStripStatusLabel _lblBotStatus = new();
    private readonly ToolStripStatusLabel _lblPrice = new();

    // Murad-bot controls
    private readonly TextBox _txtBotUrl = new();
    private readonly Button _btnBotConnect = new();
    private Label _lblBotInfo = new();

    private IDataProvider? _currentProvider;
    private MuradBotProvider? _botProvider;
    private bool _isConnectedToBot;

    public MainForm()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "ArTraV2 - Financial Chart";
        Size = new Size(1400, 900);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(19, 23, 34);
        DoubleBuffered = true;
        MinimumSize = new Size(800, 600);

        // Top toolbar
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(30, 34, 45),
            Padding = new Padding(6, 6, 6, 6)
        };

        // Data source
        _cmbDataSource.Items.AddRange(["Yahoo Finance", "Binance", "Murad Bot"]);
        _cmbDataSource.SelectedIndex = 0;
        _cmbDataSource.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbDataSource.Width = 110;
        _cmbDataSource.Location = new Point(6, 8);
        _cmbDataSource.FlatStyle = FlatStyle.Flat;

        // Symbol input
        _txtSymbol.Text = "AAPL";
        _txtSymbol.Width = 120;
        _txtSymbol.Location = new Point(124, 8);
        _txtSymbol.BorderStyle = BorderStyle.FixedSingle;
        _txtSymbol.BackColor = Color.FromArgb(42, 46, 57);
        _txtSymbol.ForeColor = Color.White;
        _txtSymbol.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };

        // Cycle selector
        _cmbCycle.Items.AddRange(["1m", "5m", "15m", "1h", "4h", "Daily", "Weekly", "Monthly"]);
        _cmbCycle.SelectedIndex = 5; // Daily
        _cmbCycle.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCycle.Width = 75;
        _cmbCycle.Location = new Point(252, 8);
        _cmbCycle.FlatStyle = FlatStyle.Flat;

        // Load button
        _btnLoad.Text = "Load";
        _btnLoad.Width = 60;
        _btnLoad.Location = new Point(335, 7);
        _btnLoad.FlatStyle = FlatStyle.Flat;
        _btnLoad.BackColor = Color.FromArgb(38, 166, 91);
        _btnLoad.ForeColor = Color.White;
        _btnLoad.Click += (s, e) => LoadData();

        // Render type
        _cmbRenderType.Items.AddRange(["Candle", "OHLC", "Line", "Area"]);
        _cmbRenderType.SelectedIndex = 0;
        _cmbRenderType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbRenderType.Width = 80;
        _cmbRenderType.Location = new Point(405, 8);
        _cmbRenderType.FlatStyle = FlatStyle.Flat;
        _cmbRenderType.SelectedIndexChanged += (s, e) =>
        {
            _chart.RenderType = (StockRenderType)_cmbRenderType.SelectedIndex;
            _chartPanel.Invalidate();
        };

        // Bot URL
        var lblBot = new Label
        {
            Text = "Bot:",
            ForeColor = Color.FromArgb(150, 155, 165),
            Location = new Point(510, 10),
            AutoSize = true
        };
        _txtBotUrl.Text = "http://localhost:8080";
        _txtBotUrl.Width = 180;
        _txtBotUrl.Location = new Point(540, 8);
        _txtBotUrl.BorderStyle = BorderStyle.FixedSingle;
        _txtBotUrl.BackColor = Color.FromArgb(42, 46, 57);
        _txtBotUrl.ForeColor = Color.White;

        _btnBotConnect.Text = "Connect";
        _btnBotConnect.Width = 70;
        _btnBotConnect.Location = new Point(728, 7);
        _btnBotConnect.FlatStyle = FlatStyle.Flat;
        _btnBotConnect.BackColor = Color.FromArgb(41, 98, 255);
        _btnBotConnect.ForeColor = Color.White;
        _btnBotConnect.Click += async (s, e) => await ConnectToBot();

        toolbar.Controls.AddRange([_cmbDataSource, _txtSymbol, _cmbCycle, _btnLoad,
            _cmbRenderType, lblBot, _txtBotUrl, _btnBotConnect]);

        // Bot info panel
        _lblBotInfo = new Label
        {
            Dock = DockStyle.Top,
            Height = 0,
            BackColor = Color.FromArgb(25, 29, 40),
            ForeColor = Color.FromArgb(209, 212, 220),
            Padding = new Padding(8, 4, 8, 4),
            Font = new Font("Segoe UI", 9f),
            Visible = false
        };

        // Chart panel
        _chartPanel.Dock = DockStyle.Fill;
        _chartPanel.BackColor = Color.FromArgb(19, 23, 34);
        _chartPanel.Paint += ChartPanel_Paint;
        _chartPanel.MouseMove += ChartPanel_MouseMove;
        _chartPanel.MouseLeave += (s, e) => { _chart.CursorPosition = null; _chartPanel.Invalidate(); };
        _chartPanel.MouseWheel += ChartPanel_MouseWheel;
        _chartPanel.Resize += (s, e) => _chartPanel.Invalidate();

        // Status bar
        _lblStatus.Text = "Ready";
        _lblStatus.ForeColor = Color.FromArgb(150, 155, 165);
        _lblStatus.Spring = true;
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        _lblBotStatus.Text = "";
        _lblBotStatus.ForeColor = Color.FromArgb(41, 98, 255);

        _lblPrice.Text = "";
        _lblPrice.ForeColor = Color.FromArgb(38, 166, 91);

        _statusStrip.BackColor = Color.FromArgb(30, 34, 45);
        _statusStrip.Items.AddRange([_lblStatus, _lblBotStatus, _lblPrice]);

        Controls.Add(_chartPanel);
        Controls.Add(_lblBotInfo);
        Controls.Add(toolbar);
        Controls.Add(_statusStrip);

        // Keyboard shortcuts
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        _chart.Render(e.Graphics, _chartPanel.ClientRectangle);
    }

    private void ChartPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        _chart.CursorPosition = e.Location;
        _chartPanel.Invalidate();
    }

    private void ChartPanel_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (e.Delta > 0)
            _chart.ZoomIn();
        else
            _chart.ZoomOut();

        _chart.ShowLatest();
        _chartPanel.Invalidate();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
                _chart.ScrollLeft(e.Control ? 20 : 5);
                _chartPanel.Invalidate();
                break;
            case Keys.Right:
                _chart.ScrollRight(e.Control ? 20 : 5);
                _chartPanel.Invalidate();
                break;
            case Keys.Home:
                _chart.ShowAll();
                _chartPanel.Invalidate();
                break;
            case Keys.End:
                _chart.ShowLatest();
                _chartPanel.Invalidate();
                break;
            case Keys.Add:
            case Keys.Oemplus:
                _chart.ZoomIn();
                _chart.ShowLatest();
                _chartPanel.Invalidate();
                break;
            case Keys.Subtract:
            case Keys.OemMinus:
                _chart.ZoomOut();
                _chart.ShowLatest();
                _chartPanel.Invalidate();
                break;
        }
    }

    private async void LoadData()
    {
        var symbol = _txtSymbol.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(symbol)) return;

        _lblStatus.Text = $"Loading {symbol}...";
        _btnLoad.Enabled = false;

        try
        {
            if (_currentProvider is IDisposable d) d.Dispose();
            _currentProvider = null;

            var dataSource = _cmbDataSource.SelectedIndex;

            if (dataSource == 2 && _botProvider != null)
            {
                _currentProvider = _botProvider;
            }
            else
            {
                _currentProvider = dataSource switch
                {
                    0 => new YahooFinanceProvider(),
                    1 => new BinanceProvider(),
                    2 => new BinanceProvider(), // fallback if bot not connected
                    _ => new YahooFinanceProvider()
                };
            }

            var cycle = GetSelectedCycle();
            var endDate = DateTime.UtcNow;
            var startDate = GetStartDate(cycle, endDate);

            var bars = await _currentProvider.GetHistoricalDataAsync(symbol, cycle, startDate, endDate);

            if (bars.Count == 0)
            {
                _lblStatus.Text = "No data found";
                return;
            }

            _chart.Data = bars;
            _chart.Symbol = symbol;
            _chart.ShowLatest();
            _chartPanel.Invalidate();

            var last = bars[^1];
            _lblPrice.Text = $"  Last: {last.Close:N4}  ";
            _lblStatus.Text = $"{symbol} - {bars.Count:N0} bars loaded ({_currentProvider.Name})";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _btnLoad.Enabled = true;
        }
    }

    private async Task ConnectToBot()
    {
        if (_isConnectedToBot)
        {
            _botProvider?.DisconnectLiveStream();
            _botProvider?.Dispose();
            _botProvider = null;
            _isConnectedToBot = false;
            _btnBotConnect.Text = "Connect";
            _btnBotConnect.BackColor = Color.FromArgb(41, 98, 255);
            _lblBotStatus.Text = "";
            _lblBotInfo.Visible = false;
            _lblBotInfo.Height = 0;
            return;
        }

        try
        {
            _lblBotStatus.Text = "Connecting...";
            _botProvider = new MuradBotProvider(_txtBotUrl.Text.Trim());

            _botProvider.OnLiveBar += bar =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => UpdateLiveBar(bar));
                    return;
                }
                UpdateLiveBar(bar);
            };

            // Test connection
            var status = await _botProvider.GetBotStatusAsync();

            if (status != null)
            {
                _isConnectedToBot = true;
                _btnBotConnect.Text = "Disconnect";
                _btnBotConnect.BackColor = Color.FromArgb(231, 76, 60);
                _lblBotStatus.Text = $"Bot: {status.Symbol ?? "ETHUSDT"} | {status.Mode ?? "DRY"}";

                _lblBotInfo.Text = $"  Murad Bot Connected | Symbol: {status.Symbol ?? "ETHUSDT"} | " +
                    $"Position: {status.Position ?? "NONE"} | PnL: {status.PnL:N2} USDT | " +
                    $"Signal: {status.Signal ?? "-"}";
                _lblBotInfo.Visible = true;
                _lblBotInfo.Height = 28;

                // Auto-select Murad Bot data source and load
                _cmbDataSource.SelectedIndex = 2;
                _txtSymbol.Text = status.Symbol ?? "ETHUSDT";

                // Start live stream
                await _botProvider.ConnectLiveStreamAsync(status.Symbol?.ToLower() ?? "ethusdt");
            }
            else
            {
                // Bot dashboard not available, but provider can still fetch from Binance
                _isConnectedToBot = true;
                _btnBotConnect.Text = "Disconnect";
                _btnBotConnect.BackColor = Color.FromArgb(231, 76, 60);
                _lblBotStatus.Text = "Bot: offline (Binance direct)";
            }
        }
        catch (Exception ex)
        {
            _lblBotStatus.Text = $"Bot error: {ex.Message}";
            _botProvider?.Dispose();
            _botProvider = null;
        }
    }

    private void UpdateLiveBar(BarData bar)
    {
        _lblPrice.Text = $"  Live: {bar.Close:N4}  ";
        _lblPrice.ForeColor = Color.FromArgb(38, 166, 91);

        if (_chart.Data.Count > 0)
        {
            var lastBar = _chart.Data[^1];
            if (lastBar.Date.Date == bar.Date.Date && lastBar.Date.Hour == bar.Date.Hour)
            {
                _chart.Data[^1] = bar;
            }
            else
            {
                _chart.Data.Add(bar);
            }
            _chart.ShowLatest();
            _chartPanel.Invalidate();
        }
    }

    private DataCycle GetSelectedCycle() => _cmbCycle.SelectedIndex switch
    {
        0 => DataCycle.Minute(1),
        1 => DataCycle.Minute(5),
        2 => DataCycle.Minute(15),
        3 => DataCycle.Hourly(1),
        4 => DataCycle.Hourly(4),
        5 => DataCycle.Daily,
        6 => DataCycle.Weekly,
        7 => DataCycle.Monthly,
        _ => DataCycle.Daily
    };

    private static DateTime GetStartDate(DataCycle cycle, DateTime end) => cycle.CycleBase switch
    {
        DataCycleBase.Minute => end.AddDays(-7),
        DataCycleBase.Hour => end.AddDays(-90),
        DataCycleBase.Day => end.AddYears(-5),
        DataCycleBase.Week => end.AddYears(-10),
        DataCycleBase.Month => end.AddYears(-20),
        _ => end.AddYears(-2)
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _botProvider?.Dispose();
            if (_currentProvider is IDisposable d)
                d.Dispose();
        }
        base.Dispose(disposing);
    }
}
