using ECommerceAuction.UserService.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

/// <summary>
/// Configures the user.Privileges table used by fine-grained RBAC authorization.
/// </summary>
public sealed class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("Privileges", "user");

        builder.HasKey(privilege => privilege.Id);

        builder.Property(privilege => privilege.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(privilege => privilege.Code)
            .HasColumnName("code")
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder.HasIndex(privilege => privilege.Code)
            .IsUnique();

        builder.Property(privilege => privilege.Name)
            .HasColumnName("name")
            .HasColumnType("nvarchar(150)")
            .IsRequired();

        builder.Property(privilege => privilege.Description)
            .HasColumnName("description")
            .HasColumnType("nvarchar(500)")
            .IsRequired(false);

        builder.Property(privilege => privilege.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(30)")
            .HasDefaultValue(PrivilegeStatuses.Active)
            .IsRequired();
    }
}
