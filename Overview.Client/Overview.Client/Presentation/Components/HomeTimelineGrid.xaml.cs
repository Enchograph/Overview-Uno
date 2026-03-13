using Overview.Client.Application.Home;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

namespace Overview.Client.Presentation.Components;

public sealed partial class HomeTimelineGrid : UserControl
{
    private const double LabelColumnWidth = 112d;
    private HomeLayoutSnapshot? snapshot;

    public event EventHandler<HomeTimelineSwipeRequestedEventArgs>? SwipeRequested;

    public HomeTimelineGrid()
    {
        this.InitializeComponent();
    }

    public void Render(HomeLayoutSnapshot? newSnapshot)
    {
        snapshot = newSnapshot;
        HeaderGrid.Children.Clear();
        HeaderGrid.ColumnDefinitions.Clear();
        RowsPanel.Children.Clear();

        if (snapshot is null)
        {
            return;
        }

        var dayColumnWidth = snapshot.ViewMode == Domain.Enums.HomeViewMode.Month ? 82d : 112d;
        var cellSize = dayColumnWidth;

        RenderHeader(dayColumnWidth, cellSize);
        foreach (var timeBlock in snapshot.TimeBlocks)
        {
            RenderRow(timeBlock.Label, snapshot.Columns.Count, dayColumnWidth, cellSize);
        }
    }

    private void RenderHeader(double dayColumnWidth, double headerHeight)
    {
        HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(LabelColumnWidth)
        });

        HeaderGrid.Children.Add(BuildLabelCell(
            snapshot!.ViewMode == Domain.Enums.HomeViewMode.Month ? "Month" : "Week",
            "Time",
            LabelColumnWidth,
            headerHeight,
            isEmphasized: true));

        for (var index = 0; index < snapshot!.Columns.Count; index++)
        {
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(dayColumnWidth)
            });

            var column = snapshot.Columns[index];
            var cell = BuildLabelCell(
                column.HeaderLabel,
                column.IsToday ? "Today" : column.Date.ToString("yyyy-MM-dd"),
                dayColumnWidth,
                headerHeight,
                isToday: column.IsToday);
            Grid.SetColumn(cell, index + 1);
            HeaderGrid.Children.Add(cell);
        }
    }

    private void RenderRow(string label, int columnCount, double dayColumnWidth, double cellSize)
    {
        var rowGrid = new Grid
        {
            ColumnSpacing = 8
        };

        rowGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(LabelColumnWidth)
        });

        rowGrid.Children.Add(BuildLabelCell(label, "Block", LabelColumnWidth, cellSize));

        for (var index = 0; index < columnCount; index++)
        {
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(dayColumnWidth)
            });

            var cell = new Border
            {
                Width = dayColumnWidth,
                Height = cellSize,
                Background = ResolveBrush("LayerFillColorAltBrush", Colors.Transparent),
                BorderBrush = ResolveBrush("CardStrokeColorDefaultBrush", Colors.LightGray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18)
            };

            Grid.SetColumn(cell, index + 1);
            rowGrid.Children.Add(cell);
        }

        RowsPanel.Children.Add(rowGrid);
    }

    private static Border BuildLabelCell(
        string title,
        string caption,
        double width,
        double height,
        bool isEmphasized = false,
        bool isToday = false)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            Padding = new Thickness(12),
            Background = ResolveBrush(
                isToday ? "AccentFillColorSecondaryBrush" : "CardBackgroundFillColorSecondaryBrush",
                isToday ? Colors.LightSteelBlue : Colors.Transparent),
            BorderBrush = ResolveBrush("CardStrokeColorDefaultBrush", Colors.LightGray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18)
        };

        var stackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 4
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = new FontWeight { Weight = (ushort)(isEmphasized ? 700 : 600) },
            TextWrapping = TextWrapping.WrapWholeWords
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = caption,
            Opacity = 0.72,
            Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"],
            TextWrapping = TextWrapping.WrapWholeWords
        });

        border.Child = stackPanel;
        return border;
    }

    private void OnRootManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        const double swipeThreshold = 96d;

        if (e.Cumulative.Translation.X >= swipeThreshold)
        {
            SwipeRequested?.Invoke(this, new HomeTimelineSwipeRequestedEventArgs(isPrevious: true));
        }
        else if (e.Cumulative.Translation.X <= -swipeThreshold)
        {
            SwipeRequested?.Invoke(this, new HomeTimelineSwipeRequestedEventArgs(isPrevious: false));
        }
    }

    private static Brush ResolveBrush(string resourceKey, Color fallbackColor)
    {
        if (Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue(resourceKey, out var resource) &&
            resource is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(fallbackColor);
    }
}
