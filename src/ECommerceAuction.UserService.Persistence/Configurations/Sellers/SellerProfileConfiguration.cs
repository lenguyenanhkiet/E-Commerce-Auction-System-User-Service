using ECommerceAuction.UserService.Domain.Sellers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Configurations.Sellers
{
    public sealed class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
    {
        public void Configure(EntityTypeBuilder<SellerProfile> builder)
        {
            builder.ToTable("SellerProfiles", "user");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.SellerType).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

            builder.Property(x => x.BusinessName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.TaxCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BusinessLicenseUrl).HasMaxLength(500).IsRequired();

            builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
            builder.Property(x => x.BankAccountNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BankName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.BankAccountHolder).HasMaxLength(255).IsRequired();

            builder.Property(x => x.RejectReason).HasMaxLength(1000);

            // One active/latest application row per user is enforced in the repository
            // (HasActiveApplicationAsync), not via a unique index, since a user can have
            // multiple historical (Rejected) rows over time.
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);

            builder.HasMany(x => x.History)
                .WithOne()
                .HasForeignKey(x => x.SellerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
