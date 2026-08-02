using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Reputation;

public sealed class UserRatingConfiguration : IEntityTypeConfiguration<UserRating>
{
    public void Configure(EntityTypeBuilder<UserRating> builder)
    {
        builder.ToTable("UserRatings", "user", table =>
            table.HasCheckConstraint("CK_UserRatings_score", "[score] >= 1 AND [score] <= 5"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.TransactionId).HasColumnName("transaction_id");
        builder.Property(x => x.RaterUserId).HasColumnName("rater_user_id");
        builder.Property(x => x.TargetUserId).HasColumnName("target_user_id");
        builder.Property(x => x.Score).HasColumnName("score");
        builder.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(UserRating.MaxCommentLength);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(x => new { x.TransactionType, x.TransactionId, x.RaterUserId, x.TargetUserId }).IsUnique();
        builder.HasIndex(x => new { x.RaterUserId, x.CreatedAt });
        builder.HasIndex(x => new { x.TargetUserId, x.CreatedAt });
    }
}
