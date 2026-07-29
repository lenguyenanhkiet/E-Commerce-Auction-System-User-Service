using ECommerceAuction.UserService.Domain.Sellers.Applications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Configurations.Sellers
{
    public sealed class SellerApplicationHistoryConfiguration : IEntityTypeConfiguration<SellerApplicationHistory>
    {
        public void Configure(EntityTypeBuilder<SellerApplicationHistory> builder)
        {
            builder.ToTable("SellerApplicationHistories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.FromStatus).HasMaxLength(20);
            builder.Property(x => x.ToStatus).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasIndex(x => x.SellerProfileId);
        }
    }
}
