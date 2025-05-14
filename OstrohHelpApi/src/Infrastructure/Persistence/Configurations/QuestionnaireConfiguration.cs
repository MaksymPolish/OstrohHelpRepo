using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Users.Roles;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionnaireConfiguration : IEntityTypeConfiguration<Questionary>
{
    public void Configure(EntityTypeBuilder<Questionary> builder)
    {
        builder.ToTable("Questionnaires");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, value => new QuestionaryId(value));

        builder.Property(q => q.Description)
            .HasColumnType("text");

        builder.Property(q => q.SubmittedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();
        
        
        // FK
        builder.HasOne<User>(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<QuestionaryStatuses>(q => q.Status)
            .WithMany()
            .HasForeignKey(q => q.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}