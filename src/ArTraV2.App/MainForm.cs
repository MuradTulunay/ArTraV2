using ArTraV2.Core.Chart;
using ArTraV2.Core.Chart.Drawing;
using ArTraV2.Core.DataProviders;
using ArTraV2.Core.Formula;
using ArTraV2.Core.Indicators;
using ArTraV2.Core.Interfaces;
using ArTraV2.Core.Models;
using ArTraV2.App.Controls;
using ArTraV2.App.Dialogs;

namespace ArTraV2.App;

public class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        UpdateStyles();
    }
}

public partial class MainForm : Form
{
    private readonly ChartRenderer _chart = new();
    private readonly DoubleBufferedPanel _chartPanel = new();
    private readonly ComboBox _cmbDataSource = new();
    private readonly TextBox _txtSymbol = new();
    private readonly Button _btnLoad = new();
    private readonly ComboBox _cmbCycle = new();
    private readonly ComboBox _cmbRenderType = new();
    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _lblStatus = new();
    private readonly ToolStripStatusLabel _lblBotStatus = new();
    private readonly ToolStripStatusLabel _lblPrice = new();
    private readonly ToolStripStatusLabel _lblLive = new();

    // Murad-bot controls
    private readonly TextBox _txtBotUrl = new();
    private readonly Button _btnBotConnect = new();
    private readonly Button _btnIndicators = new();
    private Label _lblBotInfo = new();

    // Drawing & indicators
    private readonly DrawingManager _drawingManager = new();
    private readonly DrawingToolbar _drawingToolbar = new();

    private IDataProvider? _currentProvider;
    private ILiveDataProvider? _liveProvider;
    private MuradBotProvider? _botProvider;
    private bool _isConnectedToBot;
    private string _currentSymbol = "";
    private DataCycle _currentCycle = DataCycle.Daily;

    public MainForm()
    {
        InitializeUI();
        // Register built-in formula assembly so indicators are discoverable
        ArTraV2.Core.Formula.FormulaBase.RegAssembly("BuiltIn",
            typeof(ArTraV2.Core.Formula.FormulaBase).Assembly);
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
        _txtBotUrl.Text = "http://178.104.110.229:8081";
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

        // Indicators / Formula Editor — unified button
        _btnIndicators.Text = "Indicators";
        _btnIndicators.Width = 90;
        _btnIndicators.Location = new Point(810, 7);
        _btnIndicators.FlatStyle = FlatStyle.Flat;
        _btnIndicators.BackColor = Color.FromArgb(42, 46, 57);
        _btnIndicators.ForeColor = Color.White;
        _btnIndicators.Click += (s, e) => OpenFormulaEditor();

        toolbar.Controls.AddRange([_cmbDataSource, _txtSymbol, _cmbCycle, _btnLoad,
            _cmbRenderType, lblBot, _txtBotUrl, _btnBotConnect, _btnIndicators]);

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

        // Drawing toolbar (left side)
        _drawingToolbar.ToolSelected += tool =>
        {
            _drawingManager.CancelCreation();
            _drawingManager.ActiveTool = tool;
            _drawingManager.SelectedObject = null;
            _chartPanel.Cursor = tool.HasValue ? Cursors.Cross : Cursors.Default;
            _chartPanel.Invalidate();
        };

        // Chart panel
        _chartPanel.Dock = DockStyle.Fill;
        _chartPanel.BackColor = Color.FromArgb(19, 23, 34);
        _chartPanel.Paint += ChartPanel_Paint;
        _chartPanel.MouseDown += ChartPanel_MouseDown;
        _chartPanel.MouseMove += ChartPanel_MouseMove;
        _chartPanel.MouseUp += ChartPanel_MouseUp;
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

        _lblLive.Text = "";
        _lblLive.ForeColor = Color.FromArgb(255, 152, 0);

        _lblPrice.Text = "";
        _lblPrice.ForeColor = Color.FromArgb(38, 166, 91);

        _statusStrip.BackColor = Color.FromArgb(30, 34, 45);
        _statusStrip.Items.AddRange([_lblStatus, _lblLive, _lblBotStatus, _lblPrice]);

        Controls.Add(_chartPanel);
        Controls.Add(_drawingToolbar);
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

        // Render drawing objects on top
        if (_chart.Data.Count > 0 && _chart.Layout.Panes.Count > 0)
        {
            _drawingManager.RenderAll(e.Graphics, AnchorToScreen);
        }
    }

    private void ChartPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _chart.Data.Count == 0) return;

        var barIndex = _chart.GetBarIndexAtX(_chartPanel.ClientRectangle, e.X);
        var pane = _chart.GetPaneAt(e.Y) ?? _chart.Layout.MainPane;
        var price = pane.YToPrice(e.Y);

        var toolFinished = _drawingManager.OnMouseDown(e.Location, barIndex, price, AnchorToScreen);
        if (toolFinished)
        {
            _drawingToolbar.ResetToCrosshair();
            _chartPanel.Cursor = Cursors.Default;
        }

        _chartPanel.Invalidate();
    }

    private void ChartPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        _chart.CursorPosition = e.Location;

        if (_drawingManager.IsDragging && _chart.Data.Count > 0)
        {
            var barIndex = _chart.GetBarIndexAtX(_chartPanel.ClientRectangle, e.X);
            var pane = _chart.GetPaneAt(e.Y) ?? _chart.Layout.MainPane;
            var price = pane.YToPrice(e.Y);
            _drawingManager.OnMouseMove(e.Location, barIndex, price);
        }

        _chartPanel.Invalidate();
    }

    private void ChartPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        _drawingManager.OnMouseUp();
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

    private PointF AnchorToScreen(DrawingAnchor anchor)
    {
        var pane = _chart.Layout.MainPane;
        var visibleIdx = anchor.BarIndex - _chart.StartIndex;
        var barW = (float)pane.Bounds.Width / _chart.VisibleBars;
        var x = pane.Bounds.Left + visibleIdx * barW + barW / 2;
        var y = pane.PriceToY(anchor.Price);
        return new PointF(x, y);
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Delete:
                _drawingManager.DeleteSelected();
                _chartPanel.Invalidate();
                break;
            case Keys.Escape:
                _drawingManager.CancelCreation();
                _drawingToolbar.ResetToCrosshair();
                _chartPanel.Cursor = Cursors.Default;
                _chartPanel.Invalidate();
                break;
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

    // --- Indicator Selector ---

    private void OpenFormulaEditor()
    {
        using var dlg = new FormulaEditorDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedFormulaName != null)
        {
            // User selected "Add to Chart" — add the formula as an indicator
            var formula = FormulaBase.CreateByName(dlg.SelectedFormulaName);
            if (formula != null)
            {
                var adapter = new FormulaIndicatorAdapter(formula);
                _chart.ActiveIndicators.Add(adapter);
                _chartPanel.Invalidate();
            }
        }
        else
        {
            // Dialog closed normally — refresh chart in case formulas were compiled
            _chartPanel.Invalidate();
        }
    }

    private async void LoadData()
    {
        var symbol = _txtSymbol.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(symbol)) return;

        _lblStatus.Text = $"Loading {symbol}...";
        _btnLoad.Enabled = false;

        // Clear previous chart immediately
        StopLiveStream();
        _chart.Data = [];
        _chart.Symbol = symbol;
        _chart.CursorPosition = null;
        _chart.CursorBarIndex = null;
        _lblPrice.Text = "";
        _lblLive.Text = "";
        _chartPanel.Invalidate();

        try
        {
            if (_currentProvider is IDisposable d && _currentProvider != _botProvider)
                d.Dispose();
            _currentProvider = null;

            var dataSource = _cmbDataSource.SelectedIndex;

            if (dataSource == 2)
            {
                // Murad Bot — always use MuradBotProvider, create if needed
                _botProvider ??= new MuradBotProvider();
                _currentProvider = _botProvider;
            }
            else
            {
                _currentProvider = dataSource switch
                {
                    0 => new YahooFinanceProvider(),
                    1 => new BinanceProvider(),
                    _ => new YahooFinanceProvider()
                };
            }

            _currentCycle = GetSelectedCycle();
            _currentSymbol = symbol;
            var endDate = DateTime.UtcNow;
            var startDate = GetStartDate(_currentCycle, endDate);

            var bars = await _currentProvider.GetHistoricalDataAsync(symbol, _currentCycle, startDate, endDate);

            if (bars.Count == 0)
            {
                _lblStatus.Text = $"No data found for {symbol}";
                return;
            }

            _chart.Data = bars;
            _chart.Symbol = symbol;
            _chart.ShowLatest();
            _chartPanel.Invalidate();

            var last = bars[^1];
            _lblPrice.Text = $"  Last: {last.Close:N4}  ";
            _lblStatus.Text = $"{symbol} - {bars.Count:N0} bars loaded ({_currentProvider.Name})";

            // Auto-start live stream for supported providers
            await StartLiveStream();
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

    // --- Live Data Stream ---

    private async Task StartLiveStream()
    {
        // Determine which live provider to use
        if (_currentProvider is ILiveDataProvider liveProv)
        {
            _liveProvider = liveProv;
        }
        else if (_currentProvider is YahooFinanceProvider)
        {
            // Yahoo doesn't have WebSocket — no live stream
            _lblLive.Text = "";
            return;
        }
        else
        {
            return;
        }

        try
        {
            _liveProvider.OnLiveBar += OnLiveBarReceived;

            var interval = CycleToWsInterval(_currentCycle);
            await _liveProvider.ConnectAsync(_currentSymbol, interval);

            _lblLive.Text = "  LIVE  ";
            _lblLive.ForeColor = Color.FromArgb(38, 166, 91);
            _lblStatus.Text += " | Live connected";
        }
        catch (Exception ex)
        {
            _lblLive.Text = "  LIVE OFF  ";
            _lblLive.ForeColor = Color.FromArgb(231, 76, 60);
            _lblStatus.Text += $" | Live failed: {ex.Message}";
        }
    }

    private void StopLiveStream()
    {
        if (_liveProvider != null)
        {
            _liveProvider.OnLiveBar -= OnLiveBarReceived;
            _liveProvider.Disconnect();
            _liveProvider = null;
        }
        _lblLive.Text = "";
    }

    private void OnLiveBarReceived(BarData bar)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnLiveBarReceived(bar));
            return;
        }

        UpdateLiveBar(bar);
    }

    private void UpdateLiveBar(BarData bar)
    {
        // Update price label with color based on direction
        if (_chart.Data.Count > 0)
        {
            var prevClose = _chart.Data[^1].Close;
            var isUp = bar.Close >= prevClose;
            _lblPrice.ForeColor = isUp ? Color.FromArgb(38, 166, 91) : Color.FromArgb(231, 76, 60);
        }
        _lblPrice.Text = $"  {bar.Close:N4}  ";

        if (_chart.Data.Count == 0) return;

        // Determine if this bar belongs to the same candle or is a new one
        var lastBar = _chart.Data[^1];
        if (IsSameCandle(lastBar.Date, bar.Date, _currentCycle))
        {
            // Update current candle
            _chart.Data[^1] = lastBar with
            {
                High = Math.Max(lastBar.High, bar.High),
                Low = Math.Min(lastBar.Low, bar.Low),
                Close = bar.Close,
                Volume = bar.Volume > 0 ? bar.Volume : lastBar.Volume
            };
        }
        else
        {
            // New candle
            _chart.Data.Add(bar);
            _chart.ShowLatest();
        }

        _chartPanel.Invalidate();
    }

    private static bool IsSameCandle(DateTime existing, DateTime incoming, DataCycle cycle)
    {
        return cycle.CycleBase switch
        {
            DataCycleBase.Minute => existing.Date == incoming.Date
                && existing.Hour == incoming.Hour
                && existing.Minute / cycle.Multiplier == incoming.Minute / cycle.Multiplier,
            DataCycleBase.Hour => existing.Date == incoming.Date
                && existing.Hour / cycle.Multiplier == incoming.Hour / cycle.Multiplier,
            DataCycleBase.Day => existing.Date == incoming.Date,
            DataCycleBase.Week => GetWeekNumber(existing) == GetWeekNumber(incoming)
                && existing.Year == incoming.Year,
            DataCycleBase.Month => existing.Year == incoming.Year
                && existing.Month == incoming.Month,
            _ => existing.Date == incoming.Date
        };
    }

    private static int GetWeekNumber(DateTime dt)
    {
        return System.Globalization.CultureInfo.InvariantCulture.Calendar
            .GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static string CycleToWsInterval(DataCycle cycle) => cycle.CycleBase switch
    {
        DataCycleBase.Minute => $"{cycle.Multiplier}m",
        DataCycleBase.Hour => $"{cycle.Multiplier}h",
        DataCycleBase.Day => "1d",
        DataCycleBase.Week => "1w",
        DataCycleBase.Month => "1M",
        _ => "1d"
    };

    // --- Bot Connection ---

    private async Task ConnectToBot()
    {
        if (_isConnectedToBot)
        {
            _botProvider?.Disconnect();
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
                LoadData(); // This will auto-start live stream via MuradBotProvider
            }
            else
            {
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
            StopLiveStream();
            _botProvider?.Dispose();
            if (_currentProvider is IDisposable d && _currentProvider != _botProvider)
                d.Dispose();
        }
        base.Dispose(disposing);
    }
}
