using ECommerceAuction.UserService.Domain.Reputation.Seller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Reputation;

public sealed class SellerReputationProfileConfiguration
    : IEntityTypeConfiguration<SellerReputationProfile>
{
    public void Configure(EntityTypeBuilder<SellerReputationProfile> builder)
    {
        builder.ToTable("SellerReputationProfiles", "user");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.ConfirmedScore).HasColumnName("confirmed_score");
        builder.Ignore(x => x.TrustLevel);
        builder.Property(x => x.SellingRestrictionStatus)
            .HasColumnName("selling_restriction_status")
            .HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.Property(x => x.AuctionRestrictionStatus)
            .HasColumnName("auction_restriction_status")
            .HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.Property(x => x.RestrictedUntil).HasColumnName("restricted_until");
        builder.Property(x => x.RequiresManualReview)
            .HasColumnName("requires_manual_review");
        builder.Property(x => x.BlockingViolationCode)
            .HasColumnName("blocking_violation_code")
            .HasMaxLength(100).IsUnicode(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
