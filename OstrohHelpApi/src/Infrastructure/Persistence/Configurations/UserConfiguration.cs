using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Users.Roles; // Ensure this namespace is included

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasConversion(u => u.Value, u => new UserId(u));

        builder.Property(u => u.GoogleId)
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.Course)
            .HasColumnType("text");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255);

        builder.Property(u => u.AuthToken)
            .HasMaxLength(255);

        builder.Property(u => u.TokenExpiration)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();

        // Foreign Key
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // або NoAction
    }
}