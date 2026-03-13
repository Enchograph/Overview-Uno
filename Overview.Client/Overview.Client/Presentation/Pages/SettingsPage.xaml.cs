using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsPageViewModel ViewModel => (SettingsPageViewModel)DataContext;

    public SettingsPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<SettingsPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
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
    }
}
