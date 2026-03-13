using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Components;
using Overview.Client.Presentation.Layout;
using Microsoft.UI.Xaml.Navigation;

namespace Overview.Client.Presentation.Pages;

public sealed partial class HomePage : Page
{
    private HomePageViewModel ViewModel => (HomePageViewModel)DataContext;

    public HomePage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<HomePageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout(ActualWidth);
        await ViewModel.InitializeAsync(ActualWidth).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
    }

    private async void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout(e.NewSize.Width);
        await ViewModel.UpdateViewportAsync(e.NewSize.Width).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnPreviousPeriodButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.NavigatePreviousAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnNextPeriodButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.NavigateNextAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnWeekViewButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SetViewModeAsync(HomeViewMode.Week).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnMonthViewButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SetViewModeAsync(HomeViewMode.Month).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnPeriodTitleButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.TogglePicker();
        if (ViewModel.IsPickerOpen)
        {
            await TimeSelectionPicker.InitializeAsync(
                ViewModel.CurrentSelectionMode,
                ViewModel.CurrentReferenceDate).ConfigureAwait(true);
        }

        ApplyViewModelState();
    }

    private async void OnTimeSelectionConfirmed(object sender, TimeSelectionConfirmedEventArgs e)
    {
        await ViewModel.ApplyConfirmedPeriodAsync(e.SelectedPeriod).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnTimelineGridSwipeRequested(object sender, HomeTimelineSwipeRequestedEventArgs e)
    {
        if (e.IsPrevious)
        {
            await ViewModel.NavigatePreviousAsync().ConfigureAwait(true);
        }
        else
        {
            await ViewModel.NavigateNextAsync().ConfigureAwait(true);
        }

        ApplyViewModelState();
    }

    private async void OnTimelineGridInteractionRequested(object sender, HomeTimelineInteractionRequestedEventArgs e)
    {
        if (!e.Interaction.IsWithinGrid)
        {
            return;
        }

        if (e.IsHold)
        {
            if (e.Interaction.HitItemId is Guid itemId)
            {
                Frame?.Navigate(typeof(AddItemPage), new AddItemNavigationRequest
                {
                    EditItemId = itemId
                });
            }
            else
            {
                Frame?.Navigate(typeof(AddItemPage), new AddItemNavigationRequest
                {
                    SuggestedStartDate = e.Interaction.ColumnDate,
                    SuggestedStartTime = e.Interaction.CellStartTime
                });
            }

            return;
        }

        if (e.Interaction.HitItemId is Guid detailItemId)
        {
            await ViewModel.ShowDetailAsync(detailItemId).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private void OnCloseDetailButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseDetail();
        ApplyViewModelState();
    }

    private void OnDetailEditRequested(object sender, Guid itemId)
    {
        ViewModel.CloseDetail();
        ApplyViewModelState();
        Frame?.Navigate(typeof(AddItemPage), new AddItemNavigationRequest
        {
            EditItemId = itemId
        });
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is AddItemNavigationRequest)
        {
            ViewModel.CloseDetail();
        }
    }

    private void ApplyViewModelState()
    {
        PageTitleTextBlock.Text = ViewModel.Title;
        PageDescriptionTextBlock.Text = ViewModel.Description;
        PeriodTitleTextBlock.Text = ViewModel.PeriodTitle;
        VisibleRangeTextBlock.Text = ViewModel.VisibleRangeSummary;
        ViewModeSummaryTextBlock.Text = ViewModel.ViewModeSummary;
        GridSummaryTextBlock.Text = ViewModel.GridSummary;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        ItemDetailCard.DataContext = ViewModel.Detail;

        WeekViewButton.IsEnabled = !ViewModel.IsBusy && ViewModel.CurrentViewMode != HomeViewMode.Week;
        MonthViewButton.IsEnabled = !ViewModel.IsBusy && ViewModel.SupportsMonthView && ViewModel.CurrentViewMode != HomeViewMode.Month;
        MonthViewButton.Visibility = ViewModel.SupportsMonthView ? Visibility.Visible : Visibility.Collapsed;
        PreviousPeriodButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        NextPeriodButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        PeriodTitleButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        TimePickerHost.Visibility = ViewModel.IsPickerOpen ? Visibility.Visible : Visibility.Collapsed;
        DetailOverlay.Visibility = ViewModel.IsDetailOpen ? Visibility.Visible : Visibility.Collapsed;

        if (ViewModel.IsAuthenticated && ViewModel.Snapshot is not null)
        {
            LoggedOutStateBorder.Visibility = Visibility.Collapsed;
            TimelineGrid.Visibility = Visibility.Visible;
            TimelineGrid.Render(ViewModel.Snapshot);
        }
        else
        {
            TimelineGrid.Visibility = Visibility.Collapsed;
            TimelineGrid.Render(null);
            LoggedOutStateBorder.Visibility = Visibility.Visible;
        }
    }

    private void ApplyAdaptiveLayout(double width)
    {
        var useDualPane = AdaptiveLayout.UseDualPane(width);

        LayoutRoot.ColumnDefinitions[0].Width = useDualPane ? new GridLength(360) : new GridLength(1, GridUnitType.Star);
        LayoutRoot.ColumnDefinitions[1].Width = useDualPane ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        LayoutRoot.RowDefinitions[0].Height = useDualPane ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
        LayoutRoot.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

        ControlPanel.MaxWidth = useDualPane ? 360 : double.PositiveInfinity;

        Grid.SetRow(ControlPanel, 0);
        Grid.SetColumn(ControlPanel, 0);
        Grid.SetRowSpan(ControlPanel, useDualPane ? 2 : 1);

        Grid.SetRow(TimelineHost, useDualPane ? 0 : 1);
        Grid.SetColumn(TimelineHost, useDualPane ? 1 : 0);
        Grid.SetRowSpan(TimelineHost, useDualPane ? 2 : 1);
    }
}
