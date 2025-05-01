using Microsoft.EntityFrameworkCore;
using Domain.Messages;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MessageConfiguration :IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Text)
            .HasColumnType("text");

        builder.Property(m => m.SentAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // FK
        builder.HasOne(m => m.Consultations)
            .WithMany()
            .HasForeignKey(m => m.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}