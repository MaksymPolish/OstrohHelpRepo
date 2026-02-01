using Domain.Users;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Users.Roles;
using Domain.Users.Tockens; // Ensure this namespace is included

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

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.Course)
            .HasColumnType("text");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>();

        // Foreign Key for roles (з навігаційною властивістю для Include)
        builder.HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //Key for user tokens
        builder.HasMany<UserToken>()
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}