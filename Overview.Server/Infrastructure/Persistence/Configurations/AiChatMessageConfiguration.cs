using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overview.Server.Domain.Entities;
using Overview.Server.Infrastructure.Persistence.Converters;

namespace Overview.Server.Infrastructure.Persistence.Configurations;

public sealed class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("ai_chat_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Role).HasConversion<string>().HasMaxLength(32);
        builder.Property(message => message.RequestType).HasConversion<string>().HasMaxLength(32);
        builder.Property(message => message.Message).HasMaxLength(8000);
        builder.Property(message => message.LinkedItemIds)
            .HasJsonbConversion();

        builder.HasIndex(message => new { message.UserId, message.OccurredOn, message.CreatedAt });
    }
}
