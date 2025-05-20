using Domain.Conferences.Statuses;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConsultationStatusConfiguration : IEntityTypeConfiguration<ConsultationStatuses>
{
    public void Configure(EntityTypeBuilder<ConsultationStatuses> builder)
    {
        builder.ToTable("ConsultationStatuses");

        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id)
            .HasConversion(
                id => id.Value,
                value => new ConsultationStatusesId(value))
            .IsRequired();

        builder.Property(cs => cs.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}