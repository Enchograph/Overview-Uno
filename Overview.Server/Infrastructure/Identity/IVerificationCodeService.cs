using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Infrastructure.Identity;

public interface IVerificationCodeService
{
    Task<AuthVerificationCode> CreateRegistrationCodeAsync(string email, CancellationToken cancellationToken);

    Task<AuthVerificationCode?> ValidateRegistrationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken);
}
