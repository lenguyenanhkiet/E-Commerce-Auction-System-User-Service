using ECommerceAuction.UserService.Domain.Entities.IdentityVerification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users
{
    public sealed class IdentityVerificationConfiguration : IEntityTypeConfiguration<IdentityVerification>
    {
        public void Configure(EntityTypeBuilder<IdentityVerification> builder)
        {
            builder.ToTable("IdentityVerification", "user");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.IdentityNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.IdentityFrontImageUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.IdentityBackImageUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.VerifiedBy).HasMaxLength(100);
            builder.Property(x => x.ConfidenceScore).HasColumnType("decimal(5,4)");

            builder.HasIndex(x => x.UserId).IsUnique();

        }
    }
}
