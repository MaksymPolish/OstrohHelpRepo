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
        builder.Property(ma => ma.FileUrl).IsRequired();
        builder.Property(ma => ma.FileType).IsRequired();
        builder.Property(ma => ma.CreatedAt).IsRequired();

        // Конвертація для ValueObject MessageId
        builder.Property(ma => ma.MessageId)
            .HasConversion(
                id => id.Value,
                value => new MessageId(value))
            .IsRequired();

        builder.HasOne(ma => ma.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
