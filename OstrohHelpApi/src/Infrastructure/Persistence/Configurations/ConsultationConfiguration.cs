using Domain.Consultations;
using Domain.Inventory;
using Domain.Users;
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
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new ConsultationsId(value));

        builder.Property(c => c.ScheduledTime)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();

        builder.Property(c => c.CreatedAt)
            .HasConversion<DateTimeUtcConverter>()
            .HasDefaultValueSql("timezone('utc', now())");

        // FK
        builder.HasOne(c => c.Questionary)
            .WithMany()
            .HasForeignKey(c => c.QuestionnaireId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(c => c.QuestionnaireId)
            .HasConversion(c => c.Value, c => new QuestionaryId(c))
            .HasColumnType("uuid");

        //User 1 - Student
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.StudentId)
            .IsRequired()
            .HasConversion(c => c.Value, c => new UserId(c))
            .HasColumnType("uuid");

        //User 2 - Psychologist
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.PsychologistId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.PsychologistId)
            .IsRequired()
            .HasConversion(x => x.Value, x => new UserId(x))
            .HasColumnType("uuid");

        //Status of Consultation
        builder.HasOne(c => c.ConsultationStatuses)
            .WithMany()
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.StatusId)
            .IsRequired()
            .HasConversion(c => c.Value, c => new ConsultationStatusesId(c))
            .HasColumnType("uuid");
    }
}