using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Domain.Entities;
using Overview.Server.Infrastructure.Persistence.Converters;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.Title).HasMaxLength(200);
        builder.Property(item => item.Description).HasMaxLength(4000);
        builder.Property(item => item.Location).HasMaxLength(500);
        builder.Property(item => item.Color).HasMaxLength(32);
        builder.Property(item => item.TimeZoneId).HasMaxLength(100);
        builder.Property(item => item.SourceDeviceId).HasMaxLength(200);
        builder.Property(item => item.ReminderConfig)
            .HasJsonbConversion();
        builder.Property(item => item.RepeatRule)
            .HasJsonbConversion();

        builder.HasIndex(item => new { item.UserId, item.LastModifiedAt });
        builder.HasIndex(item => new { item.UserId, item.Type });
        builder.HasIndex(item => new { item.UserId, item.DeletedAt });
    }
}
