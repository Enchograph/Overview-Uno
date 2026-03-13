using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Domain.Entities;
using Overview.Server.Infrastructure.Persistence.Converters;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class SyncChangeConfiguration : IEntityTypeConfiguration<SyncChange>
{
    public void Configure(EntityTypeBuilder<SyncChange> builder)
    {
        builder.ToTable("sync_changes");

        builder.HasKey(change => change.Id);

        builder.Property(change => change.DeviceId).HasMaxLength(200);
        builder.Property(change => change.EntityType).HasConversion<string>().HasMaxLength(32);
        builder.Property(change => change.ChangeType).HasConversion<string>().HasMaxLength(32);
        builder.Property(change => change.ItemSnapshot)
            .HasJsonbConversion();
        builder.Property(change => change.SettingsSnapshot)
            .HasJsonbConversion();

        builder.HasIndex(change => new { change.UserId, change.CreatedAt });
        builder.HasIndex(change => new { change.UserId, change.SyncedAt });
        builder.HasIndex(change => new { change.UserId, change.EntityType, change.EntityId });
    }
}
