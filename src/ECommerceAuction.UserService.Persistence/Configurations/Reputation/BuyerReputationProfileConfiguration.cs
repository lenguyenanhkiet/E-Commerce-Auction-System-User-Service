using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Reputation;

public sealed class BuyerReputationProfileConfiguration
    : IEntityTypeConfiguration<BuyerReputationProfile>
{
    public void Configure(EntityTypeBuilder<BuyerReputationProfile> builder)
    {
        builder.ToTable("BuyerReputationProfiles", "user");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.ConfirmedScore).HasColumnName("confirmed_score").HasColumnType("int");
        builder.Property(x => x.PendingScore).HasColumnName("pending_score").HasColumnType("int");

        builder.Property(x => x.LifetimeEarnedPoints).HasColumnName("lifetime_earned_points").HasColumnType("bigint");
        builder.Property(x => x.LifetimePenaltyPoints).HasColumnName("lifetime_penalty_points").HasColumnType("bigint");

        builder.Property(x => x.SuccessfulTransactions).HasColumnName("successful_transactions").HasColumnType("int");
        builder.Property(x => x.FailedTransactions).HasColumnName("failed_transactions").HasColumnType("int");
        builder.Property(x => x.SuccessfulAuctions).HasColumnName("successful_auctions").HasColumnType("int");
        builder.Property(x => x.FailedAuctions).HasColumnName("failed_auctions").HasColumnType("int");
        builder.Property(x => x.PenaltyCount).HasColumnName("penalty_count").HasColumnType("int");

        builder.Property(x => x.TrustLevel)
            .HasColumnName("trust_level")
            .HasColumnType("nvarchar(30)")
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2(3)");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetime2(3)");
    }
}
