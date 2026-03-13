using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
