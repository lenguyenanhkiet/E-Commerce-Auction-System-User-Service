using ECommerceAuction.UserService.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures authentication/security audit logs.
/// </summary>
public sealed class UserAuditLogConfiguration : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> builder)
    {
        builder.ToTable("UserAuditLogs", "user");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(auditLog => auditLog.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.TargetUserId)
            .HasColumnName("target_user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.Action)
            .HasColumnName("action")
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("nvarchar(100)")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.OldValue)
            .HasColumnName("old_value")
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.NewValue)
            .HasColumnName("new_value")
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("nvarchar(50)")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(auditLog => auditLog.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetimeoffset(3)")
            .IsRequired();

        builder.HasIndex(auditLog => auditLog.Action);
        builder.HasIndex(auditLog => auditLog.TargetUserId);
    }
}
