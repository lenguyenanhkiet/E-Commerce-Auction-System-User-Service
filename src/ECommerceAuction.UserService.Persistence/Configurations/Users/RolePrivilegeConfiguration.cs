using ECommerceAuction.UserService.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures the current many-to-many assignments between roles and privileges.
/// </summary>
public sealed class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("RolePrivileges", "user");

        builder.HasKey(rolePrivilege => rolePrivilege.Id);

        builder.Property(rolePrivilege => rolePrivilege.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(rolePrivilege => rolePrivilege.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(rolePrivilege => rolePrivilege.PrivilegeId)
            .HasColumnName("privilege_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(rolePrivilege => rolePrivilege.AssignedBy)
            .HasColumnName("assigned_by")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(rolePrivilege => rolePrivilege.AssignedAt)
            .HasColumnName("assigned_at")
            .HasColumnType("datetimeoffset(3)")
            .IsRequired();

        // Keep one current assignment per role and privilege.
        builder.HasIndex(rolePrivilege => new
            {
                rolePrivilege.RoleId,
                rolePrivilege.PrivilegeId
            })
            .IsUnique();

        builder.HasOne(rolePrivilege => rolePrivilege.Role)
            .WithMany(role => role.RolePrivileges)
            .HasForeignKey(rolePrivilege => rolePrivilege.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rolePrivilege => rolePrivilege.Privilege)
            .WithMany()
            .HasForeignKey(rolePrivilege => rolePrivilege.PrivilegeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
