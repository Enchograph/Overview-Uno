using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Overview.Client.Application.Auth;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class LoginPage : Page
{
    private LoginPageViewModel ViewModel => (LoginPageViewModel)DataContext;

    public LoginPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<LoginPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ViewModel.AuthenticationSucceeded += OnAuthenticationSucceeded;
        ApplyViewModelState();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();

        var restored = await ViewModel.RestoreSessionAsync().ConfigureAwait(true);
        if (!restored)
        {
            ApplyViewModelState();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.AuthenticationSucceeded -= OnAuthenticationSucceeded;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private async void OnSubmitButtonClick(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
        await ViewModel.SubmitAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnSendCodeButtonClick(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
        await ViewModel.SendVerificationCodeAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnToggleModeButtonClick(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ViewModel.ToggleMode();
        ApplyViewModelState();
    }

    private void OnAuthenticationSucceeded(object? sender, AuthSession session)
    {
        if (Frame?.CurrentSourcePageType != typeof(ShellPage))
        {
            Frame?.Navigate(typeof(ShellPage));
        }
    }

    private void SyncViewModelFromInputs()
    {
        ViewModel.BaseUrl = BaseUrlTextBox.Text;
        ViewModel.Email = EmailTextBox.Text;
        ViewModel.Password = PasswordBox.Password;
        ViewModel.VerificationCode = VerificationCodeTextBox.Text;
    }

    private void ApplyViewModelState()
    {
        BaseUrlTextBox.Text = ViewModel.BaseUrl;
        EmailTextBox.Text = ViewModel.Email;
        PasswordBox.Password = ViewModel.Password;
        VerificationCodeTextBox.Text = ViewModel.VerificationCode;

        VerificationCodeTextBox.Visibility = ViewModel.IsRegisterMode ? Visibility.Visible : Visibility.Collapsed;
        SendCodeButton.Visibility = ViewModel.IsRegisterMode ? Visibility.Visible : Visibility.Collapsed;
        SubmitButton.Content = ViewModel.IsRegisterMode ? "Register" : "Login";
        ToggleModeButton.Content = ViewModel.IsRegisterMode
            ? "Already have an account? Login"
            : "Need an account? Register";
        SubtitleTextBlock.Text = ViewModel.IsRegisterMode
            ? "Create an account, request a code, and complete registration."
            : "Sign in to sync your plans across devices.";
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        FormPanel.IsHitTestVisible = !ViewModel.IsBusy;
        FormPanel.Opacity = ViewModel.IsBusy ? 0.72 : 1;
    }
}
