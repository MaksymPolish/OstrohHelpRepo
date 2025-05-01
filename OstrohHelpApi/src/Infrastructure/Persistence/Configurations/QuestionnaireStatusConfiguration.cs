using Domain.Questionnaires.Statuses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations;

public class QuestionnaireStatusConfiguration : IEntityTypeConfiguration<QuestionnaireStatuses>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<QuestionnaireStatuses> builder)
    {
        builder.ToTable("QuestionnaireStatuses");

        builder.HasKey(qs => qs.Id);

        builder.Property(qs => qs.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}