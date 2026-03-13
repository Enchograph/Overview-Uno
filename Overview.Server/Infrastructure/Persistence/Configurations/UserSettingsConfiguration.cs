using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Domain.Entities;
using Overview.Server.Infrastructure.Persistence.Converters;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(settings => settings.Id);

        builder.Property(settings => settings.ThemeMode).HasConversion<string>().HasMaxLength(32);
        builder.Property(settings => settings.WeekStartDay).HasConversion<string>().HasMaxLength(16);
        builder.Property(settings => settings.HomeViewMode).HasConversion<string>().HasMaxLength(32);
        builder.Property(settings => settings.ListPageDefaultTab).HasConversion<string>().HasMaxLength(32);
        builder.Property(settings => settings.ListPageSortBy).HasConversion<string>().HasMaxLength(32);
        builder.Property(settings => settings.Language).HasMaxLength(16);
        builder.Property(settings => settings.ThemePreset).HasMaxLength(64);
        builder.Property(settings => settings.ListPageTheme).HasMaxLength(64);
        builder.Property(settings => settings.ListManualOrder)
            .HasJsonbConversion();
        builder.Property(settings => settings.AiBaseUrl).HasMaxLength(500);
        builder.Property(settings => settings.AiApiKey).HasMaxLength(500);
        builder.Property(settings => settings.AiModel).HasMaxLength(200);
        builder.Property(settings => settings.SyncServerBaseUrl).HasMaxLength(500);
        builder.Property(settings => settings.TimeZoneId).HasMaxLength(100);
        builder.Property(settings => settings.SourceDeviceId).HasMaxLength(200);
        builder.Property(settings => settings.WidgetPreferences)
            .HasJsonbConversion();

        builder.HasIndex(settings => settings.UserId)
            .IsUnique();

        builder.HasIndex(settings => new { settings.UserId, settings.LastModifiedAt });
    }
}
