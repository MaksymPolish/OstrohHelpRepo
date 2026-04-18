using Domain.Inventory.Statuses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations;

public class QuestionnaireStatusConfiguration : IEntityTypeConfiguration<QuestionaryStatuses>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<QuestionaryStatuses> builder)
    {
        builder.ToTable("QuestionnaireStatuses");

        builder.HasKey(qs => qs.Id);

        builder.Property(qs => qs.Id)
            .IsRequired();

        builder.Property(qs => qs.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}