using Domain.Consultations;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultations>
{
    public void Configure(EntityTypeBuilder<Consultations> builder)
    {
        builder.ToTable("Consultations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ScheduledTime)
            .HasColumnType("datetime2")
            .HasConversion<DateTimeUtcConverter>();

        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime2")
            .HasConversion<DateTimeUtcConverter>()
            .HasDefaultValueSql("GETUTCDATE()");

        // FK
        builder.HasOne(c => c.Questionnaire)
            .WithMany()
            .HasForeignKey(c => c.QuestionnaireId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.PsychologistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ConsultationStatuses)
            .WithMany()
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}