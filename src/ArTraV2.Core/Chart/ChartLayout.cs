using System.Drawing;

namespace ArTraV2.Core.Chart;

public class ChartLayout
{
    public List<ChartPane> Panes { get; } = [];
    public const int SeparatorHeight = 3;
    public const int RightMargin = 70;
    public const int BottomMargin = 25;
    public const int TopMargin = 10;

    public ChartPane MainPane => Panes.FirstOrDefault(p => p.IsMainPane) ?? Panes[0];

    public void Clear()
    {
        Panes.Clear();
    }

    public ChartPane AddMainPane()
    {
        var pane = new ChartPane { IsMainPane = true, HeightRatio = 3f, Title = "" };
        Panes.Insert(0, pane);
        return pane;
    }

    public ChartPane AddSubPane(string title, float heightRatio = 1f)
    {
        var pane = new ChartPane { IsMainPane = false, HeightRatio = heightRatio, Title = title };
        Panes.Add(pane);
        return pane;
    }

    public void RecalculateLayout(Rectangle totalBounds)
    {
        if (Panes.Count == 0) return;

        var chartWidth = totalBounds.Width - RightMargin;
        var availableHeight = totalBounds.Height - BottomMargin - TopMargin
            - SeparatorHeight * (Panes.Count - 1);

        var totalRatio = Panes.Sum(p => p.HeightRatio);
        var y = totalBounds.Y + TopMargin;

        for (int i = 0; i < Panes.Count; i++)
        {
            var pane = Panes[i];
            var height = (int)(availableHeight * pane.HeightRatio / totalRatio);

            pane.Bounds = new Rectangle(totalBounds.X, y, chartWidth, height);
            y += height;

            if (i < Panes.Count - 1)
                y += SeparatorHeight;
        }
    }

    public ChartPane? GetPaneAt(float y)
    {
        foreach (var pane in Panes)
            if (pane.ContainsY(y))
                return pane;
        return null;
    }
}
