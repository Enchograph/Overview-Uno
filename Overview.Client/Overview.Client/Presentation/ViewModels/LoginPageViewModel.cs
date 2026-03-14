using Overview.Client.Application.Auth;

namespace Overview.Client.Presentation.ViewModels;

public sealed class LoginPageViewModel
{
    private readonly IAuthenticationService authenticationService;

    public LoginPageViewModel(IAuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
    }

    public string BaseUrl { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string VerificationCode { get; set; } = string.Empty;

    public bool IsRegisterMode { get; private set; }

    public bool IsBusy { get; private set; }

    public string StatusMessage { get; private set; } = string.Empty;

    public event EventHandler<AuthSession>? AuthenticationSucceeded;

    public void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        StatusMessage = string.Empty;
    }

    public async Task<bool> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteBusyActionAsync(
            async () =>
            {
                StatusMessage = "Restoring session...";
                var session = await authenticationService.RestoreSessionAsync(cancellationToken).ConfigureAwait(false);
                if (session is null)
                {
                    StatusMessage = string.Empty;
                    return false;
                }

                BaseUrl = session.BaseUrl;
                Email = session.Email;
                Password = string.Empty;
                VerificationCode = string.Empty;
                StatusMessage = "Session restored.";
                AuthenticationSucceeded?.Invoke(this, session);
                return true;
            }).ConfigureAwait(false);
    }

    public async Task SendVerificationCodeAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                await authenticationService.SendVerificationCodeAsync(
                    BaseUrl.Trim(),
                    Email.Trim(),
                    cancellationToken).ConfigureAwait(false);
                StatusMessage = "Verification code sent.";
                return true;
            }).ConfigureAwait(false);
    }

    public async Task<bool> SubmitAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteBusyActionAsync(
            async () =>
            {
                var session = IsRegisterMode
                    ? await authenticationService.RegisterAsync(
                        BaseUrl.Trim(),
                        Email.Trim(),
                        Password,
                        VerificationCode.Trim(),
                        cancellationToken).ConfigureAwait(false)
                    : await authenticationService.LoginAsync(
                        BaseUrl.Trim(),
                        Email.Trim(),
                        Password,
                        cancellationToken).ConfigureAwait(false);

                BaseUrl = session.BaseUrl;
                Email = session.Email;
                Password = string.Empty;
                VerificationCode = string.Empty;
                StatusMessage = IsRegisterMode ? "Registration succeeded." : "Login succeeded.";
                AuthenticationSucceeded?.Invoke(this, session);
                return true;
            }).ConfigureAwait(false);
    }

    public async Task<bool> SubmitOfflineAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteBusyActionAsync(
            async () =>
            {
                var session = await authenticationService.LoginOfflineAsync(cancellationToken).ConfigureAwait(false);
                BaseUrl = string.Empty;
                Email = session.Email;
                Password = string.Empty;
                VerificationCode = string.Empty;
                StatusMessage = "Offline mode enabled. Your data stays on this device.";
                AuthenticationSucceeded?.Invoke(this, session);
                return true;
            }).ConfigureAwait(false);
    }

    private async Task<bool> ExecuteBusyActionAsync(Func<Task<bool>> action)
    {
        if (IsBusy)
        {
            return false;
        }

        try
        {
            IsBusy = true;
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
