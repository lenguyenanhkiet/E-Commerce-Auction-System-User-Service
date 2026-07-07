using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures the user.UserRoles join table between users and roles.
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", "user");

        builder.HasKey(userRole => userRole.Id);

        builder.Property(userRole => userRole.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();
        builder.Property(userRole => userRole.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(userRole => userRole.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(userRole => userRole.AssignedBy)
            .HasColumnName("assigned_by")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(userRole => userRole.AssignedAt)
            .HasColumnName("assigned_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(userRole => userRole.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Property(userRole => userRole.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(30)")
            .HasDefaultValue(UserRoleStatuses.Active)
            .IsRequired();

        builder.HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique()
            .HasFilter("[revoked_at] IS NULL");
    }
}
