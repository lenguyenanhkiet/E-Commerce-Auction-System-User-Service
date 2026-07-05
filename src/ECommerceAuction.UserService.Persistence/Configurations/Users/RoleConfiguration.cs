using ECommerceAuction.UserService.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures the user.Roles table used by RBAC.
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "user");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(role => role.Code)
            .HasColumnName("code")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.HasIndex(role => role.Code)
            .IsUnique();

        builder.Property(role => role.Name)
            .HasColumnName("name")
            .HasColumnType("nvarchar(150)")
            .IsRequired();

        builder.Property(role => role.Description)
            .HasColumnName("description")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(role => role.IsSystemRole)
            .HasColumnName("is_system_role")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(role => role.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(30)")
            .HasDefaultValue(RoleStatuses.Active)
            .IsRequired();

        builder.Property(role => role.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(role => role.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(3)")
            .IsRequired();

        builder.Property(role => role.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.Navigation(role => role.RolePrivileges)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
