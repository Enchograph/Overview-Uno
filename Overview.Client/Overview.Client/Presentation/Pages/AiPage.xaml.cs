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

    private void OnComposerTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.UpdateDraft(ComposerTextBox.Text);
        ApplyViewModelState();
    }

    private async void OnSendButtonClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SendAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void ApplyViewModelState()
    {
        PageTitleTextBlock.Text = ViewModel.Title;
        PageDescriptionTextBlock.Text = ViewModel.Description;
        CurrentDayTextBlock.Text = ViewModel.CurrentDayTitle;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        MessagesListView.ItemsSource = ViewModel.Messages;
        EmptyStateBorder.Visibility = ViewModel.Messages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SendButton.IsEnabled = ViewModel.CanSend;

        if (ComposerTextBox.Text != ViewModel.DraftMessage)
        {
            ComposerTextBox.Text = ViewModel.DraftMessage;
        }

        if (ViewModel.Messages.Count > 0)
        {
            MessagesListView.ScrollIntoView(ViewModel.Messages[^1]);
        }
    }
}
