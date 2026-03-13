using Overview.Client.Domain.Entities;

namespace Overview.Client.Application.Settings;

public interface IUserSettingsService
{
    Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserSettings> SaveAsync(
        Guid userId,
        UserSettingsUpdateRequest request,
        CancellationToken cancellationToken = default);
}
