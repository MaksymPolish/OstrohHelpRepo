using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Users.Roles;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionnaireConfiguration : IEntityTypeConfiguration<Questionnaire>
{
    public void Configure(EntityTypeBuilder<Questionnaire> builder)
    {
        builder.ToTable("Questionnaires");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Description)
            .HasColumnType("text");

        builder.Property(q => q.SubmittedAt)
            .HasColumnType("datetime2")
            .HasConversion<DateTimeUtcConverter>();
        // FK
        builder.HasOne<User>(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<QuestionnaireStatuses>(q => q.Status)
            .WithMany()
            .HasForeignKey(q => q.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}