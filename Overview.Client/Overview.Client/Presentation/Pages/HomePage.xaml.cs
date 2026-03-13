using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Components;

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

        WeekViewButton.IsEnabled = !ViewModel.IsBusy && ViewModel.CurrentViewMode != HomeViewMode.Week;
        MonthViewButton.IsEnabled = !ViewModel.IsBusy && ViewModel.SupportsMonthView && ViewModel.CurrentViewMode != HomeViewMode.Month;
        MonthViewButton.Visibility = ViewModel.SupportsMonthView ? Visibility.Visible : Visibility.Collapsed;
        PreviousPeriodButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        NextPeriodButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        PeriodTitleButton.IsEnabled = !ViewModel.IsBusy && ViewModel.IsAuthenticated;
        TimePickerHost.Visibility = ViewModel.IsPickerOpen ? Visibility.Visible : Visibility.Collapsed;

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
}
