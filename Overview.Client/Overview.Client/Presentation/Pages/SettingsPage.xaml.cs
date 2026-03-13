using Overview.Client.Presentation.ViewModels;
using Overview.Client.Presentation.Layout;

namespace Overview.Client.Presentation.Pages;

public sealed partial class SettingsPage : Page
{
    private string? initialSectionKey;

    private SettingsPageViewModel ViewModel => (SettingsPageViewModel)DataContext;

    public SettingsPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<SettingsPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        ViewModel.ViewStateChanged += OnViewModelStateChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout(ActualWidth);
        await ViewModel.InitializeAsync(initialSectionKey).ConfigureAwait(true);
        ApplyViewModelState();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        initialSectionKey = e.Parameter as string;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ViewStateChanged -= OnViewModelStateChanged;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout(e.NewSize.Width);
    }

    private async void OnRefreshButtonClick(object sender, RoutedEventArgs e)
    {
        ApplyViewModelState();
        await ViewModel.RefreshAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateBack();
        ApplyViewModelState();
    }

    private void OnSectionButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string sectionKey })
        {
            ViewModel.OpenSection(sectionKey);
            ApplyViewModelState();
        }
    }

    private void OnAiBaseUrlTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.UpdateAiDraft(
            AiBaseUrlTextBox.Text,
            AiApiKeyPasswordBox.Password,
            AiModelTextBox.Text);
    }

    private void OnAiApiKeyPasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateAiDraft(
            AiBaseUrlTextBox.Text,
            AiApiKeyPasswordBox.Password,
            AiModelTextBox.Text);
    }

    private void OnAiModelTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.UpdateAiDraft(
            AiBaseUrlTextBox.Text,
            AiApiKeyPasswordBox.Password,
            AiModelTextBox.Text);
    }

    private async void OnSaveAiSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        ApplyViewModelState();
        await ViewModel.SaveAiSettingsAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnRunManualSyncButtonClick(object sender, RoutedEventArgs e)
    {
        ApplyViewModelState();
        await ViewModel.RunManualSyncAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        _ = DispatcherQueue.TryEnqueue(ApplyViewModelState);
    }

    private void ApplyViewModelState()
    {
        PageTitleTextBlock.Text = ViewModel.PageTitle;
        PageSubtitleTextBlock.Text = ViewModel.PageSubtitle;
        SessionSummaryTextBlock.Text = ViewModel.SessionSummary;
        RootIntroTextBlock.Text = ViewModel.RootIntro;
        DetailLeadTextBlock.Text = ViewModel.DetailLead;
        DetailFootnoteTextBlock.Text = ViewModel.DetailFootnote;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        SectionItemsControl.ItemsSource = ViewModel.Sections;
        DetailItemsControl.ItemsSource = ViewModel.ActiveFields;

        BackButton.Visibility = ViewModel.IsRootView ? Visibility.Collapsed : Visibility.Visible;
        RootPanel.Visibility = ViewModel.IsRootView ? Visibility.Visible : Visibility.Collapsed;
        DetailPanel.Visibility = ViewModel.IsRootView ? Visibility.Collapsed : Visibility.Visible;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        RefreshButton.IsEnabled = !ViewModel.IsBusy;
        AiEditorCard.Visibility = ViewModel.IsAiEditorVisible ? Visibility.Visible : Visibility.Collapsed;
        SyncStatusCard.Visibility = ViewModel.IsSyncSectionVisible ? Visibility.Visible : Visibility.Collapsed;
        SaveAiSettingsButton.IsEnabled = ViewModel.CanSaveAiSettings;
        RunManualSyncButton.IsEnabled = ViewModel.CanRunManualSync;

        if (AiBaseUrlTextBox.Text != ViewModel.AiSettingsForm.BaseUrl)
        {
            AiBaseUrlTextBox.Text = ViewModel.AiSettingsForm.BaseUrl;
        }

        if (AiApiKeyPasswordBox.Password != ViewModel.AiSettingsForm.ApiKey)
        {
            AiApiKeyPasswordBox.Password = ViewModel.AiSettingsForm.ApiKey;
        }

        if (AiModelTextBox.Text != ViewModel.AiSettingsForm.Model)
        {
            AiModelTextBox.Text = ViewModel.AiSettingsForm.Model;
        }
    }

    private void ApplyAdaptiveLayout(double width)
    {
        LayoutRoot.Padding = new Thickness(AdaptiveLayout.IsTablet(width) ? 32 : 24);
    }
}
