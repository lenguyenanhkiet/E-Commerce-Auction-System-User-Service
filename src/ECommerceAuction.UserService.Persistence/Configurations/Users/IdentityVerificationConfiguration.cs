using ECommerceAuction.UserService.Domain.IdentityVerifications;
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
            builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Gender).HasMaxLength(20).IsRequired();
            builder.Property(x => x.DateOfBirth).IsRequired();

            builder.Property(x => x.IdentityNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.IssueDate).IsRequired();
            builder.Property(x => x.ExpiryDate).IsRequired();
            builder.Property(x => x.IssuePlace).HasMaxLength(250).IsRequired();
            builder.Property(x => x.PermanentAddress).HasMaxLength(500).IsRequired();

            builder.Property(x => x.IdentityFrontImageKey).HasMaxLength(500).IsRequired();
            builder.Property(x => x.IdentityBackImageKey).HasMaxLength(500).IsRequired();

            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.ConfidenceScore).HasColumnType("decimal(5,4)");

            builder.Property(x => x.VerifiedBy).HasMaxLength(100);
            builder.Property(x => x.SubmittedAt).IsRequired();
            builder.Property(x => x.VerifiedAt);

            builder.HasIndex(x => x.UserId).IsUnique();
        }
    }
}