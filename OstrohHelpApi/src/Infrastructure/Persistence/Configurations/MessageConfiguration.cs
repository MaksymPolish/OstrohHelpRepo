using Domain.Consultations;
using Microsoft.EntityFrameworkCore;
using Domain.Messages;
using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MessageConfiguration :IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        // ID
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => new MessageId(value))
            .IsRequired();

        // FK - ConsultationId
        builder.Property(m => m.ConsultationId)
            .HasConversion(
                id => id.Value,
                value => new ConsultationsId(value))
            .IsRequired();

        // SenderId
        builder.Property(m => m.SenderId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired()
            .HasColumnType("uuid");

        // ReceiverId
        builder.Property(m => m.ReceiverId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired()
            .HasColumnType("uuid");

        // Текст
        builder.Property(m => m.Text)
            .HasColumnType("text")
            .IsRequired();

        // Часи
        builder.Property(m => m.SentAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();

        builder.Property(m => m.DeletedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>()
            .IsRequired(false);

        // Навігація
        builder.HasOne(m => m.Consultations)
            .WithMany()
            .HasForeignKey(m => m.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}