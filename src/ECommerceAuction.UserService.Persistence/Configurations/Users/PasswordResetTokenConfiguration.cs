using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    /// <summary>
    /// Configures the PasswordResetToken entity mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", "user");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(x => x.Token)
            .HasColumnName("token")
            .HasColumnType("nvarchar(500)")
            .IsRequired();

        builder.Property(x => x.ExpiryDate)
            .HasColumnName("expiry_date")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .HasColumnName("is_used")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}