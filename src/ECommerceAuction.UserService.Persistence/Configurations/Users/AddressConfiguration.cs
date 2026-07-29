using ECommerceAuction.UserService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses", "user");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.RecipientName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.RecipientPhone).IsRequired().HasMaxLength(12);
        builder.Property(a => a.Province).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Ward).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(255);
        builder.Property(a => a.Type).HasMaxLength(50);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.UserId, a.IsDefault }).HasFilter("[DeletedAt] IS NULL");
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}