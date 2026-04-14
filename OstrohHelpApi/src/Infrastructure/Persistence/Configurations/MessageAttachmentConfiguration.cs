using Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("message_attachments");
        builder.HasKey(ma => ma.Id);
        builder.Property(ma => ma.Id).IsRequired();
        
        builder.Property(ma => ma.MessageId)
            .IsRequired(false)
            .HasColumnType("uuid");
        
        builder.Property(ma => ma.FileUrl).IsRequired();
        builder.Property(ma => ma.FileType).IsRequired();
        builder.Property(ma => ma.FileSizeBytes).IsRequired();
        builder.Property(ma => ma.CreatedAt).IsRequired();
        builder.Property(ma => ma.CloudinaryPublicId).IsRequired();

        // Create index on MessageId for better query performance
        builder.HasIndex(ma => ma.MessageId);
        
        // Note: Message navigation property exists but is not configured via EF relationships
        // due to type incompatibility (MessageId is Guid? but Message.Id is MessageId value object).
        // FK constraint is enforced at the database level.
        // Manual loading of related messages will be handled in repository queries.
    }
}
