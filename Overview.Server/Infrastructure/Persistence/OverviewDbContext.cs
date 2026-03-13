using Microsoft.EntityFrameworkCore;
using Overview.Server.Domain.Entities;
using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Infrastructure.Persistence;

public sealed class OverviewDbContext : DbContext
{
    public OverviewDbContext(DbContextOptions<OverviewDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuthUser> Users => Set<AuthUser>();

    public DbSet<AuthRefreshToken> RefreshTokens => Set<AuthRefreshToken>();

    public DbSet<AuthVerificationCode> VerificationCodes => Set<AuthVerificationCode>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    public DbSet<SyncChange> SyncChanges => Set<SyncChange>();

    public DbSet<AiChatMessage> AiChatMessages => Set<AiChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OverviewDbContext).Assembly);
    }
}
