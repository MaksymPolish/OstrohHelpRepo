using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Users;
using Domain.Users.Tockens;
using Infrastructure.Persistence.Converters;

namespace Infrastructure.Persistence.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");

        // Primary Key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new UserTockenId(value))
            .IsRequired();

        // Foreign Key to User
        builder.Property(t => t.UserId)
            .HasColumnName("UserId")
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        // Зв’язок з User
        builder.HasOne(t => t.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Refresh Token
        builder.Property(t => t.RefreshToken)
            .IsRequired()
            .HasMaxLength(1024);

        // Expiration Date
        builder.Property(t => t.ExpiresAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .HasConversion<DateTimeUtcConverter>()
            .IsRequired();
    }
}