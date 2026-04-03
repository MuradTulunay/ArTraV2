using System.Drawing;
using System.Drawing.Drawing2D;
using ArTraV2.Core.Indicators;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Chart;

public class ChartRenderer
{
    public List<BarData> Data { get; set; } = [];
    public StockRenderType RenderType { get; set; } = StockRenderType.Candle;
    public string Symbol { get; set; } = "";
    public List<IIndicator> ActiveIndicators { get; set; } = [];

    // Colors
    public Color UpColor { get; set; } = Color.FromArgb(38, 166, 91);
    public Color DownColor { get; set; } = Color.FromArgb(231, 76, 60);
    public Color BackgroundColor { get; set; } = Color.FromArgb(19, 23, 34);
    public Color GridColor { get; set; } = Color.FromArgb(42, 46, 57);
    public Color TextColor { get; set; } = Color.FromArgb(209, 212, 220);
    public Color CrosshairColor { get; set; } = Color.FromArgb(120, 123, 134);
    public Color SeparatorColor { get; set; } = Color.FromArgb(54, 58, 69);
    public Color VolumeUpColor { get; set; } = Color.FromArgb(60, 38, 166, 91);
    public Color VolumeDownColor { get; set; } = Color.FromArgb(60, 231, 76, 60);

    // View state
    public int StartIndex { get; set; }
    public int VisibleBars { get; set; } = 80;
    public int? CursorBarIndex { get; set; }
    public PointF? CursorPosition { get; set; }

    // Layout
    public ChartLayout Layout { get; } = new();
    private const int VolumeHeightPercent = 20;

    public int EndIndex => Math.Min(StartIndex + VisibleBars, Data.Count);

    public void Render(Graphics g, Rectangle bounds)
    {
        if (Data.Count == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        ClampView();
        var visibleData = Data.Skip(StartIndex).Take(VisibleBars).ToList();
        if (visibleData.Count == 0) return;

        // Calculate all indicator results
        var allResults = new List<(IIndicator Indicator, List<IndicatorResult> Results)>();
        foreach (var ind in ActiveIndicators)
        {
            var results = ind.Calculate(Data);
            allResults.Add((ind, results));
        }

        // Build layout
        RebuildLayout(allResults);
        Layout.RecalculateLayout(bounds);

        var mainPane = Layout.MainPane;

        // Scale main pane
        var priceMin = visibleData.Min(b => b.Low);
        var priceMax = visibleData.Max(b => b.High);

        // Include overlay indicators in price range
        foreach (var (ind, results) in allResults.Where(r => r.Indicator.IsOverlay))
        {
            foreach (var series in results)
            {
                for (int i = StartIndex; i < Math.Min(StartIndex + VisibleBars, series.Values.Length); i++)
                {
                    var v = series.Values[i];
                    if (double.IsNaN(v)) continue;
                    if (v < priceMin) priceMin = v;
                    if (v > priceMax) priceMax = v;
                }
            }
        }

        var pricePadding = (priceMax - priceMin) * 0.05;
        mainPane.YMin = priceMin - pricePadding;
        mainPane.YMax = priceMax + pricePadding;

        // Scale sub panes
        foreach (var pane in Layout.Panes.Where(p => !p.IsMainPane))
        {
            var vals = new List<double>();
            foreach (var series in pane.Series)
            {
                for (int i = StartIndex; i < Math.Min(StartIndex + VisibleBars, series.Values.Length); i++)
                {
                    if (!double.IsNaN(series.Values[i]))
                        vals.Add(series.Values[i]);
                }
            }
            // Include reference lines in scaling
            vals.AddRange(pane.ReferenceLines);
            pane.AutoScale(vals.ToArray());
        }

        // Render main pane
        DrawGrid(g, mainPane);
        DrawPriceAxis(g, mainPane);

        var volumeRect = new Rectangle(mainPane.Bounds.X,
            mainPane.Bounds.Bottom - (int)(mainPane.Bounds.Height * VolumeHeightPercent / 100.0),
            mainPane.Bounds.Width,
            (int)(mainPane.Bounds.Height * VolumeHeightPercent / 100.0));
        var volumeMax = visibleData.Max(b => b.Volume) * 1.1;
        DrawVolume(g, volumeRect, visibleData, volumeMax);

        switch (RenderType)
        {
            case StockRenderType.Candle: DrawCandles(g, mainPane, visibleData); break;
            case StockRenderType.OHLC: DrawOHLC(g, mainPane, visibleData); break;
            case StockRenderType.Line: DrawLine(g, mainPane, visibleData); break;
            case StockRenderType.Area: DrawArea(g, mainPane, visibleData); break;
        }

        // Draw overlay indicators on main pane
        foreach (var (ind, results) in allResults.Where(r => r.Indicator.IsOverlay))
            foreach (var series in results)
                DrawIndicatorSeries(g, mainPane, series);

        // Render sub panes
        foreach (var pane in Layout.Panes.Where(p => !p.IsMainPane))
        {
            DrawPaneSeparator(g, pane);
            DrawGrid(g, pane);
            DrawPriceAxis(g, pane);
            DrawReferenceLines(g, pane);
            DrawPaneTitle(g, pane);

            foreach (var series in pane.Series)
                DrawIndicatorSeries(g, pane, series);
        }

        // Date axis at bottom of last pane
        var lastPane = Layout.Panes[^1];
        DrawDateAxis(g, lastPane, bounds, visibleData);

        // Crosshair spans all panes
        if (CursorPosition.HasValue)
        {
            var anyPane = Layout.GetPaneAt(CursorPosition.Value.Y);
            if (anyPane != null)
                DrawCrosshair(g, anyPane, visibleData, bounds);
        }

        // Info overlay
        DrawInfoOverlay(g, bounds, visibleData, allResults);
    }

    private void RebuildLayout(List<(IIndicator Indicator, List<IndicatorResult> Results)> allResults)
    {
        Layout.Clear();
        Layout.AddMainPane();

        foreach (var (ind, results) in allResults.Where(r => !r.Indicator.IsOverlay))
        {
            var pane = Layout.AddSubPane(ind.ShortName);
            pane.Series = results;
            pane.ReferenceLines = ind.ReferenceLines;
        }

        // Assign overlay results to main pane for info overlay
        var mainOverlays = allResults.Where(r => r.Indicator.IsOverlay).SelectMany(r => r.Results).ToList();
        Layout.MainPane.Series = mainOverlays;
    }

    // --- Shared helpers ---

    private float BarWidth(ChartPane pane) => (float)pane.Bounds.Width / VisibleBars;

    private float BarCenterX(ChartPane pane, int visibleIndex)
        => pane.Bounds.Left + visibleIndex * BarWidth(pane) + BarWidth(pane) / 2;

    // --- Grid & Axis ---

    private void DrawGrid(Graphics g, ChartPane pane)
    {
        using var pen = new Pen(GridColor, 1) { DashStyle = DashStyle.Dot };
        int lines = pane.IsMainPane ? 6 : 4;
        for (int i = 0; i <= lines; i++)
        {
            var y = pane.Bounds.Top + pane.Bounds.Height * i / lines;
            g.DrawLine(pen, pane.Bounds.Left, y, pane.Bounds.Right, y);
        }
    }

    private void DrawPriceAxis(Graphics g, ChartPane pane)
    {
        using var font = new Font("Segoe UI", 7.5f);
        using var brush = new SolidBrush(TextColor);
        int lines = pane.IsMainPane ? 6 : 4;
        for (int i = 0; i <= lines; i++)
        {
            var y = pane.Bounds.Top + pane.Bounds.Height * i / lines;
            var price = pane.YMax - (pane.YMax - pane.YMin) * i / lines;
            g.DrawString(FormatPrice(price), font, brush, pane.Bounds.Right + 4, y - 6);
        }
    }

    private void DrawDateAxis(Graphics g, ChartPane lastPane, Rectangle bounds, List<BarData> bars)
    {
        using var font = new Font("Segoe UI", 7.5f);
        using var brush = new SolidBrush(TextColor);
        var barW = BarWidth(lastPane);
        int step = Math.Max(1, bars.Count / 6);

        for (int i = 0; i < bars.Count; i += step)
        {
            var x = lastPane.Bounds.Left + i * barW + barW / 2;
            var text = bars[i].Date.ToString("MMM dd");
            var sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, x - sz.Width / 2, lastPane.Bounds.Bottom + 4);
        }
    }

    private void DrawPaneSeparator(Graphics g, ChartPane pane)
    {
        using var pen = new Pen(SeparatorColor, 1);
        g.DrawLine(pen, pane.Bounds.Left, pane.Bounds.Top - 2, pane.Bounds.Right + ChartLayout.RightMargin, pane.Bounds.Top - 2);
    }

    private void DrawPaneTitle(Graphics g, ChartPane pane)
    {
        if (string.IsNullOrEmpty(pane.Title)) return;
        using var font = new Font("Segoe UI", 8f);
        using var brush = new SolidBrush(TextColor);
        g.DrawString(pane.Title, font, brush, pane.Bounds.Left + 4, pane.Bounds.Top + 2);
    }

    private void DrawReferenceLines(Graphics g, ChartPane pane)
    {
        using var pen = new Pen(Color.FromArgb(60, TextColor), 1) { DashStyle = DashStyle.Dash };
        using var font = new Font("Segoe UI", 7f);
        using var brush = new SolidBrush(Color.FromArgb(100, TextColor));

        foreach (var refVal in pane.ReferenceLines)
        {
            var y = pane.PriceToY(refVal);
            if (y >= pane.Bounds.Top && y <= pane.Bounds.Bottom)
            {
                g.DrawLine(pen, pane.Bounds.Left, y, pane.Bounds.Right, y);
                g.DrawString(refVal.ToString("F0"), font, brush, pane.Bounds.Right + 4, y - 6);
            }
        }
    }

    // --- Price rendering ---

    private void DrawCandles(Graphics g, ChartPane pane, List<BarData> bars)
    {
        var barW = BarWidth(pane);
        var bodyW = Math.Max(1, barW * 0.7f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var isUp = bar.Close >= bar.Open;
            var color = isUp ? UpColor : DownColor;
            var x = BarCenterX(pane, i);

            using var pen = new Pen(color, 1);
            g.DrawLine(pen, x, pane.PriceToY(bar.High), x, pane.PriceToY(bar.Low));

            var openY = pane.PriceToY(bar.Open);
            var closeY = pane.PriceToY(bar.Close);
            var bodyTop = Math.Min(openY, closeY);
            var bodyHeight = Math.Max(1, Math.Abs(openY - closeY));

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, x - bodyW / 2, bodyTop, bodyW, bodyHeight);
        }
    }

    private void DrawOHLC(Graphics g, ChartPane pane, List<BarData> bars)
    {
        var barW = BarWidth(pane);
        var tickW = Math.Max(2, barW * 0.3f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var color = bar.Close >= bar.Open ? UpColor : DownColor;
            var x = BarCenterX(pane, i);

            using var pen = new Pen(color, 1.5f);
            g.DrawLine(pen, x, pane.PriceToY(bar.High), x, pane.PriceToY(bar.Low));
            g.DrawLine(pen, x - tickW, pane.PriceToY(bar.Open), x, pane.PriceToY(bar.Open));
            g.DrawLine(pen, x, pane.PriceToY(bar.Close), x + tickW, pane.PriceToY(bar.Close));
        }
    }

    private void DrawLine(Graphics g, ChartPane pane, List<BarData> bars)
    {
        if (bars.Count < 2) return;
        var points = new PointF[bars.Count];
        for (int i = 0; i < bars.Count; i++)
            points[i] = new PointF(BarCenterX(pane, i), pane.PriceToY(bars[i].Close));

        using var pen = new Pen(Color.FromArgb(41, 98, 255), 2);
        g.DrawLines(pen, points);
    }

    private void DrawArea(Graphics g, ChartPane pane, List<BarData> bars)
    {
        if (bars.Count < 2) return;
        var lineColor = Color.FromArgb(41, 98, 255);
        var points = new PointF[bars.Count + 2];

        for (int i = 0; i < bars.Count; i++)
            points[i] = new PointF(BarCenterX(pane, i), pane.PriceToY(bars[i].Close));
        points[bars.Count] = new PointF(points[bars.Count - 1].X, pane.Bounds.Bottom);
        points[bars.Count + 1] = new PointF(points[0].X, pane.Bounds.Bottom);

        using var brush = new LinearGradientBrush(
            new Point(0, pane.Bounds.Top), new Point(0, pane.Bounds.Bottom),
            Color.FromArgb(80, lineColor), Color.FromArgb(5, lineColor));
        g.FillPolygon(brush, points);

        using var pen = new Pen(lineColor, 2);
        g.DrawLines(pen, points.Take(bars.Count).ToArray());
    }

    private void DrawVolume(Graphics g, Rectangle rect, List<BarData> bars, double maxVol)
    {
        if (maxVol <= 0) return;
        var mainPane = Layout.MainPane;
        var barW = BarWidth(mainPane);
        var bodyW = Math.Max(1, barW * 0.7f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var color = bar.Close >= bar.Open ? VolumeUpColor : VolumeDownColor;
            var x = BarCenterX(mainPane, i);
            var height = (float)(bar.Volume / maxVol * rect.Height);

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, x - bodyW / 2, rect.Bottom - height, bodyW, height);
        }
    }

    // --- Indicator rendering ---

    private void DrawIndicatorSeries(Graphics g, ChartPane pane, IndicatorResult series)
    {
        var barW = BarWidth(pane);
        var start = StartIndex;
        var end = Math.Min(start + VisibleBars, series.Values.Length);

        switch (series.RenderType)
        {
            case IndicatorRenderType.Histogram:
                DrawHistogram(g, pane, series, start, end);
                break;
            case IndicatorRenderType.Line:
            case IndicatorRenderType.DottedLine:
                DrawLineSeries(g, pane, series, start, end);
                break;
        }
    }

    private void DrawLineSeries(Graphics g, ChartPane pane, IndicatorResult series, int start, int end)
    {
        var points = new List<PointF>();
        using var pen = new Pen(series.Color, series.LineWidth);
        if (series.RenderType == IndicatorRenderType.DottedLine)
            pen.DashStyle = DashStyle.Dash;

        for (int i = start; i < end; i++)
        {
            if (double.IsNaN(series.Values[i]))
            {
                if (points.Count >= 2) g.DrawLines(pen, points.ToArray());
                points.Clear();
                continue;
            }
            points.Add(new PointF(BarCenterX(pane, i - start), pane.PriceToY(series.Values[i])));
        }

        if (points.Count >= 2) g.DrawLines(pen, points.ToArray());
    }

    private void DrawHistogram(Graphics g, ChartPane pane, IndicatorResult series, int start, int end)
    {
        var barW = BarWidth(pane);
        var bodyW = Math.Max(1, barW * 0.5f);
        var zeroY = pane.PriceToY(0);

        for (int i = start; i < end; i++)
        {
            if (double.IsNaN(series.Values[i])) continue;
            var x = BarCenterX(pane, i - start);
            var valY = pane.PriceToY(series.Values[i]);
            var color = series.Values[i] >= 0
                ? Color.FromArgb(180, UpColor)
                : Color.FromArgb(180, DownColor);

            using var brush = new SolidBrush(color);
            var top = Math.Min(valY, zeroY);
            var height = Math.Abs(valY - zeroY);
            g.FillRectangle(brush, x - bodyW / 2, top, bodyW, Math.Max(1, height));
        }
    }

    // --- Crosshair ---

    private void DrawCrosshair(Graphics g, ChartPane activePane, List<BarData> bars, Rectangle bounds)
    {
        var pos = CursorPosition!.Value;
        using var pen = new Pen(CrosshairColor, 1) { DashStyle = DashStyle.Dash };

        // Vertical line across all panes
        foreach (var pane in Layout.Panes)
            g.DrawLine(pen, pos.X, pane.Bounds.Top, pos.X, pane.Bounds.Bottom);

        // Horizontal line in active pane only
        g.DrawLine(pen, activePane.Bounds.Left, pos.Y, activePane.Bounds.Right, pos.Y);

        // Price label
        var price = activePane.YToPrice(pos.Y);
        using var font = new Font("Segoe UI", 8f);
        using var bgBrush = new SolidBrush(CrosshairColor);
        using var textBrush = new SolidBrush(Color.White);
        var priceText = FormatPrice(price);
        var sz = g.MeasureString(priceText, font);
        g.FillRectangle(bgBrush, activePane.Bounds.Right + 2, pos.Y - sz.Height / 2, sz.Width + 4, sz.Height);
        g.DrawString(priceText, font, textBrush, activePane.Bounds.Right + 4, pos.Y - sz.Height / 2);

        // Date label
        var barW = BarWidth(activePane);
        var barIdx = (int)((pos.X - activePane.Bounds.Left) / barW);
        if (barIdx >= 0 && barIdx < bars.Count)
        {
            CursorBarIndex = StartIndex + barIdx;
            var dateText = bars[barIdx].Date.ToString("yyyy-MM-dd HH:mm");
            var dateSz = g.MeasureString(dateText, font);
            var lastPane = Layout.Panes[^1];
            var dateX = lastPane.Bounds.Left + barIdx * barW + barW / 2 - dateSz.Width / 2;
            g.FillRectangle(bgBrush, dateX, lastPane.Bounds.Bottom + 2, dateSz.Width + 4, dateSz.Height);
            g.DrawString(dateText, font, textBrush, dateX + 2, lastPane.Bounds.Bottom + 2);
        }
    }

    // --- Info overlay ---

    private void DrawInfoOverlay(Graphics g, Rectangle bounds, List<BarData> bars,
        List<(IIndicator Indicator, List<IndicatorResult> Results)> allResults)
    {
        int idx = CursorBarIndex.HasValue ? CursorBarIndex.Value - StartIndex : bars.Count - 1;
        if (idx < 0 || idx >= bars.Count) idx = bars.Count - 1;
        var bar = bars[idx];
        int dataIdx = StartIndex + idx;

        using var font = new Font("Segoe UI Semibold", 9f);
        using var smallFont = new Font("Segoe UI", 8.5f);
        using var labelBrush = new SolidBrush(TextColor);
        var isUp = bar.Close >= bar.Open;
        using var valueBrush = new SolidBrush(isUp ? UpColor : DownColor);

        var x = bounds.Left + 8f;
        var y = bounds.Top + 4f;

        // Symbol + OHLCV
        var symbolText = $"{Symbol}  ";
        g.DrawString(symbolText, font, labelBrush, x, y);
        x += g.MeasureString(symbolText, font).Width;

        foreach (var (label, value) in new[] {
            ("O ", FormatPrice(bar.Open)), ("H ", FormatPrice(bar.High)),
            ("L ", FormatPrice(bar.Low)), ("C ", FormatPrice(bar.Close)),
            ("V ", FormatVolume(bar.Volume)) })
        {
            g.DrawString(label, smallFont, labelBrush, x, y);
            x += g.MeasureString(label, smallFont).Width;
            g.DrawString(value, smallFont, valueBrush, x, y);
            x += g.MeasureString(value + " ", smallFont).Width;
        }

        // Overlay indicator values
        foreach (var (ind, results) in allResults.Where(r => r.Indicator.IsOverlay))
        {
            foreach (var series in results)
            {
                if (dataIdx >= 0 && dataIdx < series.Values.Length && !double.IsNaN(series.Values[dataIdx]))
                {
                    var text = $" {series.Name}: {FormatPrice(series.Values[dataIdx])} ";
                    using var indBrush = new SolidBrush(series.Color);
                    g.DrawString(text, smallFont, indBrush, x, y);
                    x += g.MeasureString(text, smallFont).Width;
                }
            }
        }

        // Sub-pane indicator values (drawn in each pane title area)
        foreach (var pane in Layout.Panes.Where(p => !p.IsMainPane))
        {
            var px = pane.Bounds.Left + 4f;
            var py = pane.Bounds.Top + 2f;

            // Title already drawn, add values after it
            using var titleFont = new Font("Segoe UI", 8f);
            px += g.MeasureString(pane.Title + "  ", titleFont).Width;

            foreach (var series in pane.Series)
            {
                if (dataIdx >= 0 && dataIdx < series.Values.Length && !double.IsNaN(series.Values[dataIdx]))
                {
                    var text = $"{FormatPrice(series.Values[dataIdx])} ";
                    using var indBrush = new SolidBrush(series.Color);
                    g.DrawString(text, titleFont, indBrush, px, py);
                    px += g.MeasureString(text, titleFont).Width;
                }
            }
        }
    }

    // --- Helpers ---

    private void ClampView()
    {
        if (StartIndex < 0) StartIndex = 0;
        if (StartIndex > Data.Count - 1) StartIndex = Data.Count - 1;
    }

    public static string FormatPrice(double price)
    {
        if (price >= 1000) return price.ToString("N2");
        if (price >= 1) return price.ToString("N4");
        return price.ToString("N8");
    }

    private static string FormatVolume(double volume)
    {
        if (volume >= 1_000_000_000) return $"{volume / 1_000_000_000:N2}B";
        if (volume >= 1_000_000) return $"{volume / 1_000_000:N2}M";
        if (volume >= 1_000) return $"{volume / 1_000:N2}K";
        return volume.ToString("N0");
    }

    public int GetBarIndexAtX(Rectangle bounds, float x)
    {
        if (Layout.Panes.Count == 0) return StartIndex;
        var barW = BarWidth(Layout.MainPane);
        return StartIndex + (int)((x - Layout.MainPane.Bounds.Left) / barW);
    }

    public ChartPane? GetPaneAt(float y) => Layout.GetPaneAt(y);

    public void ZoomIn() { VisibleBars = Math.Max(10, VisibleBars - VisibleBars / 5); }
    public void ZoomOut() { VisibleBars = Math.Min(Data.Count, VisibleBars + VisibleBars / 5); }
    public void ScrollRight(int bars = 5) { StartIndex = Math.Min(Data.Count - 1, StartIndex + bars); }
    public void ScrollLeft(int bars = 5) { StartIndex = Math.Max(0, StartIndex - bars); }

    public void ShowAll()
    {
        StartIndex = 0;
        VisibleBars = Data.Count;
    }

    public void ShowLatest()
    {
        StartIndex = Math.Max(0, Data.Count - VisibleBars);
    }
}
