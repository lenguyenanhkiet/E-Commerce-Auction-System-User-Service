using ECommerceAuction.UserService.Domain.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures refresh-token backed user sessions.
/// </summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions", "user");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(session => session.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(session => session.RefreshTokenHash)
            .HasColumnName("refresh_token_hash")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder.HasIndex(session => session.RefreshTokenHash)
            .IsUnique();

        builder.Property(session => session.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("nvarchar(100)")
            .IsRequired(false);

        builder.Property(session => session.DeviceName)
            .HasColumnName("device_name")
            .HasColumnType("nvarchar(255)")
            .IsRequired(false);

        builder.Property(session => session.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("nvarchar(50)")
            .IsRequired(false);

        builder.Property(session => session.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(session => session.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(session => session.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(session => session.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(session => session.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();
    }
}
