using ECommerceAuction.UserService.Domain.Reputation;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Users;

public class ReputationProfileConfiguration
    : IEntityTypeConfiguration<ReputationProfile>
{
    public void Configure(EntityTypeBuilder<ReputationProfile> builder)
    {
        builder.ToTable("ReputationProfiles", "user");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.TrustLevel)
            .HasColumnName("trust_level")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.TotalRatings)
            .HasColumnName("total_ratings")
            .HasColumnType("int");

        builder.Property(x => x.AverageRating)
            .HasColumnName("average_rating")
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.SuccessfulTransactions)
            .HasColumnName("successful_transactions")
            .HasColumnType("int");

        builder.Property(x => x.FailedTransactions)
            .HasColumnName("failed_transactions")
            .HasColumnType("int");

        builder.Property(x => x.SuccessfulAuctions)
            .HasColumnName("successful_auctions")
            .HasColumnType("int");

        builder.Property(x => x.FailedAuctions)
            .HasColumnName("failed_auctions")
            .HasColumnType("int");

        builder.Property(x => x.PenaltyCount)
            .HasColumnName("penalty_count")
            .HasColumnType("int");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(3)");

        builder.HasOne<User>()
            .WithOne(user => user.ReputationProfile)
            .HasForeignKey<ReputationProfile>(x => x.UserId);
    }
}
