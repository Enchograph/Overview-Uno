using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(320);

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512);

        builder.HasIndex(user => user.Email)
            .IsUnique();
    }
}
