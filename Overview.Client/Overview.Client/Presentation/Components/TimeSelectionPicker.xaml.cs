using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Presentation.ViewModels;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Text;

namespace Overview.Client.Presentation.Components;

public sealed partial class TimeSelectionPicker : UserControl
{
    private readonly TimeSelectionViewModel viewModel;

    public event EventHandler<TimeSelectionConfirmedEventArgs>? SelectionConfirmed;

    public TimeSelectionPicker()
    {
        this.InitializeComponent();
        viewModel = App.Services.Resolve<TimeSelectionViewModel>();
    }

    public async Task InitializeAsync(
        TimeSelectionMode selectionMode,
        DateOnly? initialDate = null,
        CancellationToken cancellationToken = default)
    {
        await viewModel.InitializeAsync(selectionMode, initialDate, cancellationToken).ConfigureAwait(true);
        ApplyViewModelState();
    }

    public async Task ChangeSelectionModeAsync(
        TimeSelectionMode selectionMode,
        CancellationToken cancellationToken = default)
    {
        await viewModel.ChangeSelectionModeAsync(selectionMode, cancellationToken).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnPreviousMonthButtonClick(object sender, RoutedEventArgs e)
    {
        await viewModel.MoveToPreviousMonthAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnNextMonthButtonClick(object sender, RoutedEventArgs e)
    {
        await viewModel.MoveToNextMonthAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnMonthButtonClick(object sender, RoutedEventArgs e)
    {
        await viewModel.SelectMonthAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnWeekButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CalendarPeriod weekPeriod })
        {
            await viewModel.SelectWeekAsync(weekPeriod).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private async void OnDateButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DateOnly date })
        {
            await viewModel.SelectDateAsync(date).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
    {
        if (viewModel.SelectedPeriod is not null)
        {
            SelectionConfirmed?.Invoke(this, new TimeSelectionConfirmedEventArgs(viewModel.SelectedPeriod));
        }
    }

    private async void OnRootManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        const double swipeThreshold = 72d;
        if (e.Cumulative.Translation.X >= swipeThreshold)
        {
            await viewModel.MoveToPreviousMonthAsync().ConfigureAwait(true);
            ApplyViewModelState();
        }
        else if (e.Cumulative.Translation.X <= -swipeThreshold)
        {
            await viewModel.MoveToNextMonthAsync().ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private void ApplyViewModelState()
    {
        VisibleMonthTextBlock.Text = viewModel.VisibleMonthLabel;
        MonthButtonLabelTextBlock.Text = viewModel.VisibleMonthLabel;
        StatusTextBlock.Text = viewModel.StatusMessage;
        SelectedPeriodTextBlock.Text = viewModel.SelectedPeriodLabel;
        MonthSelectionIndicator.Visibility = viewModel.MonthSelectionIndicatorVisibility;
        RenderWeekdayHeaders();
        RenderWeekRows();
    }

    private void RenderWeekdayHeaders()
    {
        WeekdayHeaderGrid.Children.Clear();
        WeekdayHeaderGrid.ColumnDefinitions.Clear();

        for (var index = 0; index < viewModel.WeekdayHeaders.Count; index++)
        {
            WeekdayHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition());

            var border = new Border
            {
                Height = 52,
                Background = ResolveBrush("LayerFillColorAltBrush", Colors.Transparent),
                CornerRadius = new CornerRadius(18)
            };

            var textBlock = new TextBlock
            {
                Text = viewModel.WeekdayHeaders[index],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = new FontWeight { Weight = 600 }
            };

            border.Child = textBlock;
            Grid.SetColumn(border, index);
            WeekdayHeaderGrid.Children.Add(border);
        }
    }

    private void RenderWeekRows()
    {
        WeekRowsPanel.Children.Clear();

        foreach (var week in viewModel.Weeks)
        {
            var rowGrid = new Grid
            {
                ColumnSpacing = 8
            };

            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
            for (var index = 0; index < 7; index++)
            {
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var weekButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Tag = week.WeekPeriod
            };
            weekButton.Click += OnWeekButtonClick;
            weekButton.Content = BuildWeekCell(week.WeekLabel, week.SelectionIndicatorVisibility);
            rowGrid.Children.Add(weekButton);

            for (var index = 0; index < week.Dates.Count; index++)
            {
                var date = week.Dates[index];
                var dateButton = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Tag = date.Date,
                    IsEnabled = date.IsEnabled,
                    Content = BuildDateCell(date)
                };
                dateButton.Click += OnDateButtonClick;
                Grid.SetColumn(dateButton, index + 1);
                rowGrid.Children.Add(dateButton);
            }

            WeekRowsPanel.Children.Add(rowGrid);
        }
    }

    private static UIElement BuildWeekCell(string label, Visibility indicatorVisibility)
    {
        var container = new Grid
        {
            Height = 72
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = ResolveBrush("LayerFillColorAltBrush", Colors.Transparent)
        };

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 6
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = label,
            TextAlignment = TextAlignment.Center,
            FontWeight = new FontWeight { Weight = 600 }
        });

        stackPanel.Children.Add(new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = ResolveBrush("AccentFillColorDefaultBrush", Colors.DodgerBlue),
            Visibility = indicatorVisibility
        });

        container.Children.Add(border);
        container.Children.Add(stackPanel);
        return container;
    }

    private static UIElement BuildDateCell(TimeSelectionDateCellViewModel date)
    {
        var container = new Grid
        {
            Height = 72
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(18),
            Opacity = date.CellOpacity,
            Background = ResolveBrush("LayerFillColorAltBrush", Colors.Transparent)
        };

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 6
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = date.DayNumberText,
            TextAlignment = TextAlignment.Center,
            FontSize = 18,
            FontWeight = new FontWeight { Weight = 600 }
        });

        stackPanel.Children.Add(new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = ResolveBrush("AccentFillColorDefaultBrush", Colors.DodgerBlue),
            Visibility = date.SelectionIndicatorVisibility
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = date.CaptionText,
            TextAlignment = TextAlignment.Center,
            Opacity = 0.72,
            Visibility = date.TodayIndicatorVisibility
        });

        container.Children.Add(border);
        container.Children.Add(stackPanel);
        return container;
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
