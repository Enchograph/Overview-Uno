using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class AuthVerificationCodeConfiguration : IEntityTypeConfiguration<AuthVerificationCode>
{
    public void Configure(EntityTypeBuilder<AuthVerificationCode> builder)
    {
        builder.ToTable("auth_verification_codes");

        builder.HasKey(code => code.Id);

        builder.Property(code => code.Email)
            .HasMaxLength(320);

        builder.Property(code => code.CodeHash)
            .HasMaxLength(128);

        builder.Property(code => code.Purpose)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(code => new { code.Email, code.Purpose, code.CreatedAt });
    }
}
