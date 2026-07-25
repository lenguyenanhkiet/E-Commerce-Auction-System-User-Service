using ECommerceAuction.UserService.Domain.Users;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users
{
    public class UserPasswordHistoryConfiguration : IEntityTypeConfiguration<UserPasswordHistory>
    {
        public void Configure(EntityTypeBuilder<UserPasswordHistory> builder)
        {
            builder.ToTable("PasswordHistories", "user");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.PasswordHash).HasMaxLength(255).IsRequired();
            builder.HasIndex(h => new { h.UserId, h.CreatedAt });
        }

    }
}
