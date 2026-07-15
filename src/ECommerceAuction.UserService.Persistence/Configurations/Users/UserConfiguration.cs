using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "user");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("nvarchar(30)")
            .IsRequired(false);

        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("[phone_number] IS NOT NULL");

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasColumnType("nvarchar(20)")
            .IsRequired(false);

        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(x => x.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(x => x.AvatarKey)
            .HasColumnName("avatar_key")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(30)")
            .HasDefaultValue("ACTIVE")
            .IsRequired();

        builder.Property(x => x.StatusExpiresAt)
            .HasColumnName("status_expires_at")
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.IsEmailConfirmed)
            .HasColumnName("email_verified")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsPhoneConfirmed)
            .HasColumnName("phone_verified")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasColumnType("int")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.AuthProvider)
            .HasColumnName("auth_provider")
            .HasColumnType("nvarchar(20)")
            .HasDefaultValue("LOCAL")
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(x => x.MustChangePassword)
            .HasColumnName("must_change_password")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.PasswordChangedAt)
            .HasColumnName("password_changed_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);
    }
}