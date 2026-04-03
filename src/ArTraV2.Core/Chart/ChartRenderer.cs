using System.Drawing;
using System.Drawing.Drawing2D;
using ArTraV2.Core.Models;

namespace ArTraV2.Core.Chart;

public class ChartRenderer
{
    public List<BarData> Data { get; set; } = [];
    public StockRenderType RenderType { get; set; } = StockRenderType.Candle;
    public string Symbol { get; set; } = "";

    // Colors
    public Color UpColor { get; set; } = Color.FromArgb(38, 166, 91);
    public Color DownColor { get; set; } = Color.FromArgb(231, 76, 60);
    public Color BackgroundColor { get; set; } = Color.FromArgb(19, 23, 34);
    public Color GridColor { get; set; } = Color.FromArgb(42, 46, 57);
    public Color TextColor { get; set; } = Color.FromArgb(209, 212, 220);
    public Color CrosshairColor { get; set; } = Color.FromArgb(120, 123, 134);
    public Color VolumeUpColor { get; set; } = Color.FromArgb(60, 38, 166, 91);
    public Color VolumeDownColor { get; set; } = Color.FromArgb(60, 231, 76, 60);

    // View state
    public int StartIndex { get; set; }
    public int VisibleBars { get; set; } = 80;
    public int? CursorBarIndex { get; set; }
    public PointF? CursorPosition { get; set; }

    // Layout
    private const int RightMargin = 70;
    private const int BottomMargin = 25;
    private const int TopMargin = 10;
    private const int VolumeHeightPercent = 20;

    public int EndIndex => Math.Min(StartIndex + VisibleBars, Data.Count);

    public void Render(Graphics g, Rectangle bounds)
    {
        if (Data.Count == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var chartRect = new Rectangle(bounds.X, bounds.Y + TopMargin,
            bounds.Width - RightMargin, bounds.Height - BottomMargin - TopMargin);
        var volumeHeight = (int)(chartRect.Height * VolumeHeightPercent / 100.0);
        var priceRect = new Rectangle(chartRect.X, chartRect.Y,
            chartRect.Width, chartRect.Height - volumeHeight);
        var volumeRect = new Rectangle(chartRect.X, priceRect.Bottom,
            chartRect.Width, volumeHeight);

        ClampView();

        var visibleData = Data.Skip(StartIndex).Take(VisibleBars).ToList();
        if (visibleData.Count == 0) return;

        var priceMin = visibleData.Min(b => b.Low);
        var priceMax = visibleData.Max(b => b.High);
        var pricePadding = (priceMax - priceMin) * 0.05;
        priceMin -= pricePadding;
        priceMax += pricePadding;

        var volumeMax = visibleData.Max(b => b.Volume) * 1.1;

        DrawGrid(g, priceRect, priceMin, priceMax);
        DrawPriceAxis(g, priceRect, bounds, priceMin, priceMax);
        DrawDateAxis(g, chartRect, bounds, visibleData);

        switch (RenderType)
        {
            case StockRenderType.Candle:
                DrawCandles(g, priceRect, visibleData, priceMin, priceMax);
                break;
            case StockRenderType.OHLC:
                DrawOHLC(g, priceRect, visibleData, priceMin, priceMax);
                break;
            case StockRenderType.Line:
                DrawLine(g, priceRect, visibleData, priceMin, priceMax);
                break;
            case StockRenderType.Area:
                DrawArea(g, priceRect, visibleData, priceMin, priceMax);
                break;
        }

        DrawVolume(g, volumeRect, visibleData, volumeMax);

        if (CursorPosition.HasValue && chartRect.Contains(Point.Round(CursorPosition.Value)))
            DrawCrosshair(g, chartRect, priceRect, visibleData, priceMin, priceMax, bounds);

        DrawInfoOverlay(g, bounds, visibleData);
    }

    private void ClampView()
    {
        if (StartIndex < 0) StartIndex = 0;
        if (StartIndex > Data.Count - 1) StartIndex = Data.Count - 1;
    }

    private float BarWidth(Rectangle rect) => (float)rect.Width / VisibleBars;

    private float PriceToY(Rectangle rect, double price, double min, double max)
    {
        if (max <= min) return rect.Top;
        return (float)(rect.Bottom - (price - min) / (max - min) * rect.Height);
    }

    private double YToPrice(Rectangle rect, float y, double min, double max)
    {
        if (rect.Height == 0) return min;
        return max - (y - rect.Top) / rect.Height * (max - min);
    }

    private void DrawGrid(Graphics g, Rectangle rect, double priceMin, double priceMax)
    {
        using var pen = new Pen(GridColor, 1) { DashStyle = DashStyle.Dot };
        int gridLines = 6;
        for (int i = 0; i <= gridLines; i++)
        {
            var y = rect.Top + rect.Height * i / gridLines;
            g.DrawLine(pen, rect.Left, y, rect.Right, y);
        }
    }

    private void DrawPriceAxis(Graphics g, Rectangle priceRect, Rectangle bounds, double priceMin, double priceMax)
    {
        using var font = new Font("Segoe UI", 8f);
        using var brush = new SolidBrush(TextColor);
        int gridLines = 6;
        for (int i = 0; i <= gridLines; i++)
        {
            var y = priceRect.Top + priceRect.Height * i / gridLines;
            var price = priceMax - (priceMax - priceMin) * i / gridLines;
            var text = FormatPrice(price);
            g.DrawString(text, font, brush, priceRect.Right + 4, y - 6);
        }
    }

    private void DrawDateAxis(Graphics g, Rectangle chartRect, Rectangle bounds, List<BarData> bars)
    {
        using var font = new Font("Segoe UI", 7.5f);
        using var brush = new SolidBrush(TextColor);
        var barW = BarWidth(chartRect);
        int step = Math.Max(1, bars.Count / 6);

        for (int i = 0; i < bars.Count; i += step)
        {
            var x = chartRect.Left + i * barW + barW / 2;
            var text = bars[i].Date.ToString("MMM dd");
            var sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, x - sz.Width / 2, chartRect.Bottom + 4);
        }
    }

    private void DrawCandles(Graphics g, Rectangle rect, List<BarData> bars, double min, double max)
    {
        var barW = BarWidth(rect);
        var bodyW = Math.Max(1, barW * 0.7f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var isUp = bar.Close >= bar.Open;
            var color = isUp ? UpColor : DownColor;
            var x = rect.Left + i * barW + barW / 2;

            var highY = PriceToY(rect, bar.High, min, max);
            var lowY = PriceToY(rect, bar.Low, min, max);
            var openY = PriceToY(rect, bar.Open, min, max);
            var closeY = PriceToY(rect, bar.Close, min, max);

            using var pen = new Pen(color, 1);
            g.DrawLine(pen, x, highY, x, lowY);

            var bodyTop = Math.Min(openY, closeY);
            var bodyHeight = Math.Max(1, Math.Abs(openY - closeY));
            var bodyRect = new RectangleF(x - bodyW / 2, bodyTop, bodyW, bodyHeight);

            if (isUp)
            {
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, bodyRect);
            }
            else
            {
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, bodyRect);
            }
        }
    }

    private void DrawOHLC(Graphics g, Rectangle rect, List<BarData> bars, double min, double max)
    {
        var barW = BarWidth(rect);
        var tickW = Math.Max(2, barW * 0.3f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var color = bar.Close >= bar.Open ? UpColor : DownColor;
            var x = rect.Left + i * barW + barW / 2;

            using var pen = new Pen(color, 1.5f);
            var highY = PriceToY(rect, bar.High, min, max);
            var lowY = PriceToY(rect, bar.Low, min, max);
            g.DrawLine(pen, x, highY, x, lowY);

            var openY = PriceToY(rect, bar.Open, min, max);
            g.DrawLine(pen, x - tickW, openY, x, openY);

            var closeY = PriceToY(rect, bar.Close, min, max);
            g.DrawLine(pen, x, closeY, x + tickW, closeY);
        }
    }

    private void DrawLine(Graphics g, Rectangle rect, List<BarData> bars, double min, double max)
    {
        if (bars.Count < 2) return;
        var barW = BarWidth(rect);
        var points = new PointF[bars.Count];

        for (int i = 0; i < bars.Count; i++)
        {
            points[i] = new PointF(
                rect.Left + i * barW + barW / 2,
                PriceToY(rect, bars[i].Close, min, max));
        }

        using var pen = new Pen(Color.FromArgb(41, 98, 255), 2);
        g.DrawLines(pen, points);
    }

    private void DrawArea(Graphics g, Rectangle rect, List<BarData> bars, double min, double max)
    {
        if (bars.Count < 2) return;
        var barW = BarWidth(rect);
        var lineColor = Color.FromArgb(41, 98, 255);

        var points = new PointF[bars.Count + 2];
        for (int i = 0; i < bars.Count; i++)
        {
            points[i] = new PointF(
                rect.Left + i * barW + barW / 2,
                PriceToY(rect, bars[i].Close, min, max));
        }
        points[bars.Count] = new PointF(points[bars.Count - 1].X, rect.Bottom);
        points[bars.Count + 1] = new PointF(points[0].X, rect.Bottom);

        using var brush = new LinearGradientBrush(
            new Point(0, rect.Top), new Point(0, rect.Bottom),
            Color.FromArgb(80, lineColor), Color.FromArgb(5, lineColor));
        g.FillPolygon(brush, points);

        using var pen = new Pen(lineColor, 2);
        g.DrawLines(pen, points.Take(bars.Count).ToArray());
    }

    private void DrawVolume(Graphics g, Rectangle rect, List<BarData> bars, double maxVol)
    {
        if (maxVol <= 0) return;
        var barW = BarWidth(rect);
        var bodyW = Math.Max(1, barW * 0.7f);

        for (int i = 0; i < bars.Count; i++)
        {
            var bar = bars[i];
            var isUp = bar.Close >= bar.Open;
            var color = isUp ? VolumeUpColor : VolumeDownColor;
            var x = rect.Left + i * barW + barW / 2;
            var height = (float)(bar.Volume / maxVol * rect.Height);
            var barRect = new RectangleF(x - bodyW / 2, rect.Bottom - height, bodyW, height);

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, barRect);
        }
    }

    private void DrawCrosshair(Graphics g, Rectangle chartRect, Rectangle priceRect,
        List<BarData> bars, double priceMin, double priceMax, Rectangle bounds)
    {
        var pos = CursorPosition!.Value;
        using var pen = new Pen(CrosshairColor, 1) { DashStyle = DashStyle.Dash };

        g.DrawLine(pen, chartRect.Left, pos.Y, chartRect.Right, pos.Y);
        g.DrawLine(pen, pos.X, chartRect.Top, pos.X, chartRect.Bottom);

        // Price label on axis
        var price = YToPrice(priceRect, pos.Y, priceMin, priceMax);
        using var font = new Font("Segoe UI", 8f);
        using var bgBrush = new SolidBrush(CrosshairColor);
        using var textBrush = new SolidBrush(Color.White);
        var priceText = FormatPrice(price);
        var sz = g.MeasureString(priceText, font);
        var labelRect = new RectangleF(priceRect.Right + 2, pos.Y - sz.Height / 2, sz.Width + 4, sz.Height);
        g.FillRectangle(bgBrush, labelRect);
        g.DrawString(priceText, font, textBrush, labelRect.X + 2, labelRect.Y);

        // Bar index from cursor position
        var barW = BarWidth(chartRect);
        var barIdx = (int)((pos.X - chartRect.Left) / barW);
        if (barIdx >= 0 && barIdx < bars.Count)
        {
            CursorBarIndex = StartIndex + barIdx;
            var bar = bars[barIdx];
            var dateText = bar.Date.ToString("yyyy-MM-dd HH:mm");
            var dateSz = g.MeasureString(dateText, font);
            var dateX = chartRect.Left + barIdx * barW + barW / 2 - dateSz.Width / 2;
            var dateLabelRect = new RectangleF(dateX, chartRect.Bottom + 2, dateSz.Width + 4, dateSz.Height);
            g.FillRectangle(bgBrush, dateLabelRect);
            g.DrawString(dateText, font, textBrush, dateLabelRect.X + 2, dateLabelRect.Y);
        }
    }

    private void DrawInfoOverlay(Graphics g, Rectangle bounds, List<BarData> bars)
    {
        int idx = CursorBarIndex.HasValue
            ? CursorBarIndex.Value - StartIndex
            : bars.Count - 1;

        if (idx < 0 || idx >= bars.Count) idx = bars.Count - 1;
        var bar = bars[idx];

        using var font = new Font("Segoe UI Semibold", 9f);
        using var labelBrush = new SolidBrush(TextColor);
        var isUp = bar.Close >= bar.Open;
        using var valueBrush = new SolidBrush(isUp ? UpColor : DownColor);

        var x = bounds.Left + 8f;
        var y = bounds.Top + 4f;

        var symbolText = $"{Symbol}  ";
        g.DrawString(symbolText, font, labelBrush, x, y);
        x += g.MeasureString(symbolText, font).Width;

        var items = new[]
        {
            ("O ", FormatPrice(bar.Open)),
            ("H ", FormatPrice(bar.High)),
            ("L ", FormatPrice(bar.Low)),
            ("C ", FormatPrice(bar.Close)),
            ("V ", FormatVolume(bar.Volume))
        };

        using var smallFont = new Font("Segoe UI", 8.5f);
        foreach (var (label, value) in items)
        {
            g.DrawString(label, smallFont, labelBrush, x, y);
            x += g.MeasureString(label, smallFont).Width;
            g.DrawString(value, smallFont, valueBrush, x, y);
            x += g.MeasureString(value + " ", smallFont).Width;
        }
    }

    private static string FormatPrice(double price)
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

    // Public helpers for mouse interaction
    public int GetBarIndexAtX(Rectangle bounds, float x)
    {
        var chartRect = new Rectangle(bounds.X, bounds.Y + TopMargin,
            bounds.Width - RightMargin, bounds.Height - BottomMargin - TopMargin);
        var barW = BarWidth(chartRect);
        return StartIndex + (int)((x - chartRect.Left) / barW);
    }

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
