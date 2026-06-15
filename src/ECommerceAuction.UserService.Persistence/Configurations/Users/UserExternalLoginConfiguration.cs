using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures external identity-provider login links such as Google OAuth2.
/// </summary>
public sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("UserExternalLogins", "user");

        builder.HasKey(externalLogin => externalLogin.Id);

        builder.Property(externalLogin => externalLogin.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(externalLogin => externalLogin.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.Provider)
            .HasColumnName("provider")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.ProviderEmail)
            .HasColumnName("provider_email")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.ProviderDisplayName)
            .HasColumnName("provider_display_name")
            .HasColumnType("nvarchar(255)")
            .IsRequired(false);

        builder.Property(externalLogin => externalLogin.AccessTokenHash)
            .HasColumnName("access_token_hash")
            .HasColumnType("nvarchar(255)")
            .IsRequired(false);

        builder.Property(externalLogin => externalLogin.RefreshTokenHash)
            .HasColumnName("refresh_token_hash")
            .HasColumnType("nvarchar(255)")
            .IsRequired(false);

        builder.Property(externalLogin => externalLogin.LinkedAt)
            .HasColumnName("linked_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(externalLogin => externalLogin.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(30)")
            .HasDefaultValue(ExternalLoginStatuses.Active)
            .IsRequired();

        builder.Property(externalLogin => externalLogin.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(externalLogin => externalLogin.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasOne(externalLogin => externalLogin.User)
            .WithMany()
            .HasForeignKey(externalLogin => externalLogin.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(externalLogin => new
            {
                externalLogin.Provider,
                externalLogin.ProviderUserId
            })
            .IsUnique();
    }
}
