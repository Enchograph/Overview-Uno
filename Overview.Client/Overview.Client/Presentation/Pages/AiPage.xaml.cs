using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Layout;
using Overview.Client.Presentation.Components;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class AiPage : Page
{
    private AiPageViewModel ViewModel => (AiPageViewModel)DataContext;

    public AiPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<AiPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout(ActualWidth);
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout(e.NewSize.Width);
    }

    private void OnComposerTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.UpdateDraft(ComposerTextBox.Text);
        ApplyViewModelState();
    }

    private async void OnCurrentPeriodButtonClick(object sender, RoutedEventArgs e)
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

    private async void OnDayModeButtonClick(object sender, RoutedEventArgs e)
    {
        await ChangeModeAsync(TimeSelectionMode.Day).ConfigureAwait(true);
    }

    private async void OnWeekModeButtonClick(object sender, RoutedEventArgs e)
    {
        await ChangeModeAsync(TimeSelectionMode.Week).ConfigureAwait(true);
    }

    private async void OnMonthModeButtonClick(object sender, RoutedEventArgs e)
    {
        await ChangeModeAsync(TimeSelectionMode.Month).ConfigureAwait(true);
    }

    private async void OnTimeSelectionConfirmed(object sender, TimeSelectionConfirmedEventArgs e)
    {
        await ViewModel.ApplyConfirmedPeriodAsync(e.SelectedPeriod).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnSendButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SendAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async Task ChangeModeAsync(TimeSelectionMode mode)
    {
        await ViewModel.SetSelectionModeAsync(mode).ConfigureAwait(true);
        if (ViewModel.IsPickerOpen)
        {
            await TimeSelectionPicker.ChangeSelectionModeAsync(mode).ConfigureAwait(true);
        }

        ApplyViewModelState();
    }

    private void ApplyViewModelState()
    {
        PageTitleTextBlock.Text = ViewModel.Title;
        PageDescriptionTextBlock.Text = ViewModel.Description;
        CurrentPeriodTextBlock.Text = ViewModel.CurrentPeriodTitle;
        VisibleRangeTextBlock.Text = ViewModel.VisibleRangeSummary;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        MessagesListView.ItemsSource = ViewModel.Messages;
        EmptyStateBorder.Visibility = ViewModel.Messages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SendButton.IsEnabled = ViewModel.CanSend;
        CurrentPeriodButton.IsEnabled = ViewModel.IsAuthenticated && !ViewModel.IsBusy;
        TimePickerHost.Visibility = ViewModel.IsPickerOpen ? Visibility.Visible : Visibility.Collapsed;
        DayModeButton.IsEnabled = !ViewModel.IsBusy && ViewModel.CurrentSelectionMode != TimeSelectionMode.Day;
        WeekModeButton.IsEnabled = !ViewModel.IsBusy && ViewModel.CurrentSelectionMode != TimeSelectionMode.Week;
        MonthModeButton.IsEnabled = !ViewModel.IsBusy && ViewModel.CurrentSelectionMode != TimeSelectionMode.Month;

        if (ComposerTextBox.Text != ViewModel.DraftMessage)
        {
            ComposerTextBox.Text = ViewModel.DraftMessage;
        }

        if (ViewModel.Messages.Count > 0)
        {
            MessagesListView.ScrollIntoView(ViewModel.Messages[^1]);
        }
    }

    private void ApplyAdaptiveLayout(double width)
    {
        var useDualPane = AdaptiveLayout.UseDualPane(width);
        var isTablet = AdaptiveLayout.IsTablet(width);

        LayoutRoot.Padding = new Thickness(isTablet ? 32 : 24);
        LayoutRoot.ColumnDefinitions[0].Width = useDualPane ? new GridLength(340) : new GridLength(1, GridUnitType.Star);
        LayoutRoot.ColumnDefinitions[1].Width = useDualPane ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        LayoutRoot.RowDefinitions[0].Height = GridLength.Auto;
        LayoutRoot.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
        LayoutRoot.RowDefinitions[2].Height = useDualPane ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;

        Grid.SetRow(SidebarPanel, 0);
        Grid.SetColumn(SidebarPanel, 0);
        Grid.SetRowSpan(SidebarPanel, useDualPane ? 3 : 1);

        Grid.SetRow(ConversationPanel, useDualPane ? 0 : 1);
        Grid.SetColumn(ConversationPanel, useDualPane ? 1 : 0);
        Grid.SetRowSpan(ConversationPanel, useDualPane ? 2 : 1);

        Grid.SetRow(ComposerPanel, useDualPane ? 2 : 2);
        Grid.SetColumn(ComposerPanel, useDualPane ? 1 : 0);
    }
}
