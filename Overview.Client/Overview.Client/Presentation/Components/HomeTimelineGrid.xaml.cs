using Overview.Client.Application.Home;
using Overview.Client.Domain.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI;
using Windows.UI.Text;

namespace Overview.Client.Presentation.Components;

public sealed partial class HomeTimelineGrid : UserControl
{
    private const double LabelColumnWidth = 112d;
    private const double ColumnSpacing = 8d;
    private const double MinimumItemHeight = 44d;
    private readonly IHomeTimelineInteractionService homeTimelineInteractionService;
    private HomeLayoutSnapshot? snapshot;
    private double currentDayColumnWidth;
    private double currentTotalHeight;
    private bool suppressNextTap;

    public event EventHandler<HomeTimelineSwipeRequestedEventArgs>? SwipeRequested;

    public event EventHandler<HomeTimelineInteractionRequestedEventArgs>? InteractionRequested;

    public HomeTimelineGrid()
    {
        this.InitializeComponent();
        homeTimelineInteractionService = App.Services.Resolve<IHomeTimelineInteractionService>();
    }

    public void Render(HomeLayoutSnapshot? newSnapshot)
    {
        snapshot = newSnapshot;
        HeaderGrid.Children.Clear();
        HeaderGrid.ColumnDefinitions.Clear();
        BodyHostGrid.Children.Clear();

        if (snapshot is null)
        {
            return;
        }

        var dayColumnWidth = snapshot.ViewMode == Domain.Enums.HomeViewMode.Month ? 82d : 112d;
        var cellSize = dayColumnWidth;
        var totalHeight = cellSize * snapshot.TimeBlocks.Count;
        currentDayColumnWidth = dayColumnWidth;
        currentTotalHeight = totalHeight;

        RenderHeader(dayColumnWidth, cellSize);
        RenderBody(dayColumnWidth, cellSize, totalHeight);
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

    private void RenderBody(double dayColumnWidth, double cellSize, double totalHeight)
    {
        var bodyGrid = new Grid
        {
            ColumnSpacing = ColumnSpacing,
            Height = totalHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        bodyGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(LabelColumnWidth)
        });

        foreach (var _ in snapshot!.Columns)
        {
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(dayColumnWidth)
            });
        }

        for (var rowIndex = 0; rowIndex < snapshot!.TimeBlocks.Count; rowIndex++)
        {
            bodyGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(cellSize)
            });

            var timeBlock = snapshot.TimeBlocks[rowIndex];
            var labelCell = BuildLabelCell(timeBlock.Label, "Block", LabelColumnWidth, cellSize);
            Grid.SetRow(labelCell, rowIndex);
            bodyGrid.Children.Add(labelCell);

            for (var columnIndex = 0; columnIndex < snapshot.Columns.Count; columnIndex++)
            {
                var cell = new Border
                {
                    Width = dayColumnWidth,
                    Height = cellSize,
                    Background = ResolveBrush("LayerFillColorAltBrush", Colors.Transparent),
                    BorderBrush = ResolveBrush("CardStrokeColorDefaultBrush", Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(18)
                };

                Grid.SetRow(cell, rowIndex);
                Grid.SetColumn(cell, columnIndex + 1);
                bodyGrid.Children.Add(cell);
            }
        }

        var dayAreaWidth = (snapshot.Columns.Count * dayColumnWidth) + ((snapshot.Columns.Count - 1) * ColumnSpacing);
        var overlayCanvas = new Canvas
        {
            Width = dayAreaWidth,
            Height = totalHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(LabelColumnWidth + ColumnSpacing, 0, 0, 0),
            IsHitTestVisible = false
        };

        foreach (var layoutItem in snapshot.Items)
        {
            var columnIndex = GetColumnIndex(layoutItem.ColumnDate);
            if (columnIndex < 0)
            {
                continue;
            }

            var itemCard = BuildItemCard(layoutItem, dayColumnWidth, totalHeight);
            Canvas.SetLeft(itemCard, columnIndex * (dayColumnWidth + ColumnSpacing));
            Canvas.SetTop(itemCard, layoutItem.TopRatio * totalHeight);
            overlayCanvas.Children.Add(itemCard);
        }

        var interactionBorder = new Border
        {
            Width = dayAreaWidth,
            Height = totalHeight,
            Background = new SolidColorBrush(Colors.Transparent),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(LabelColumnWidth + ColumnSpacing, 0, 0, 0),
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System
        };
        interactionBorder.Tapped += OnInteractionBorderTapped;
        interactionBorder.Holding += OnInteractionBorderHolding;
        interactionBorder.ManipulationCompleted += OnRootManipulationCompleted;

        BodyHostGrid.Children.Add(bodyGrid);
        BodyHostGrid.Children.Add(overlayCanvas);
        BodyHostGrid.Children.Add(interactionBorder);
        BodyHostGrid.Height = totalHeight;
        BodyHostGrid.HorizontalAlignment = HorizontalAlignment.Left;
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

    private static Border BuildItemCard(HomeLayoutItem item, double dayColumnWidth, double totalHeight)
    {
        var itemHeight = Math.Max(MinimumItemHeight, item.HeightRatio * totalHeight);
        var border = new Border
        {
            Width = dayColumnWidth,
            Height = itemHeight,
            Padding = new Thickness(10, 8, 10, 8),
            Background = BuildItemBrush(item.Type),
            BorderBrush = ResolveBrush("CardStrokeColorDefaultBrush", Colors.Transparent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Opacity = item.Opacity
        };

        var stackPanel = new StackPanel
        {
            Spacing = 4
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = item.Title,
            FontWeight = new FontWeight { Weight = 700 },
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.WrapWholeWords,
            MaxLines = itemHeight >= 72d ? 2 : 1
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = BuildItemSubtitle(item),
            Opacity = 0.78,
            Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"],
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.WrapWholeWords,
            MaxLines = itemHeight >= 84d ? 2 : 1
        });

        border.Child = stackPanel;
        return border;
    }

    private static Brush BuildItemBrush(ItemType itemType)
    {
        var color = itemType switch
        {
            ItemType.Schedule => Color.FromArgb(0xE6, 0x57, 0x8F, 0xFF),
            ItemType.Task => Color.FromArgb(0xE6, 0xFF, 0xB0, 0x20),
            _ => Color.FromArgb(0xE6, 0x56, 0xB6, 0x7A)
        };

        return new SolidColorBrush(color);
    }

    private static string BuildItemSubtitle(HomeLayoutItem item)
    {
        var startPrefix = item.IsClippedAtStart ? "..." : string.Empty;
        var endSuffix = item.IsClippedAtEnd ? " ..." : string.Empty;
        return $"{startPrefix}{item.VisibleStartAt:HH:mm} - {item.VisibleEndAt:HH:mm}{endSuffix}";
    }

    private int GetColumnIndex(DateOnly columnDate)
    {
        if (snapshot is null)
        {
            return -1;
        }

        for (var index = 0; index < snapshot.Columns.Count; index++)
        {
            if (snapshot.Columns[index].Date == columnDate)
            {
                return index;
            }
        }

        return -1;
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

    private void OnInteractionBorderTapped(object sender, TappedRoutedEventArgs e)
    {
        if (suppressNextTap)
        {
            suppressNextTap = false;
            return;
        }

        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (!TryResolveInteraction(e.GetPosition(element), out var interaction))
        {
            return;
        }

        InteractionRequested?.Invoke(this, new HomeTimelineInteractionRequestedEventArgs(interaction, isHold: false));
    }

    private void OnInteractionBorderHolding(object sender, HoldingRoutedEventArgs e)
    {
        if (!e.HoldingState.Equals(HoldingState.Started))
        {
            return;
        }

        suppressNextTap = true;

        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (!TryResolveInteraction(e.GetPosition(element), out var interaction))
        {
            return;
        }

        InteractionRequested?.Invoke(this, new HomeTimelineInteractionRequestedEventArgs(interaction, isHold: true));
    }

    private bool TryResolveInteraction(Point position, out HomeTimelineInteractionResult interaction)
    {
        interaction = HomeTimelineInteractionResult.OutsideGrid;
        if (snapshot is null || currentDayColumnWidth <= 0d || currentTotalHeight <= 0d)
        {
            return false;
        }

        if (position.X < 0d || position.Y < 0d || position.Y > currentTotalHeight)
        {
            return false;
        }

        var combinedColumnWidth = currentDayColumnWidth + ColumnSpacing;
        var columnIndex = (int)Math.Floor(position.X / combinedColumnWidth);
        if (columnIndex < 0 || columnIndex >= snapshot.Columns.Count)
        {
            return false;
        }

        var columnOffset = position.X - (columnIndex * combinedColumnWidth);
        if (columnOffset > currentDayColumnWidth)
        {
            return false;
        }

        var verticalRatio = currentTotalHeight <= 0d
            ? 0d
            : position.Y / currentTotalHeight;
        interaction = homeTimelineInteractionService.ResolveInteraction(snapshot, columnIndex, verticalRatio);
        return interaction.IsWithinGrid;
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
