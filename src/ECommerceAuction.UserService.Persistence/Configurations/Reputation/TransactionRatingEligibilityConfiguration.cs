using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Reputation;

public sealed class TransactionRatingEligibilityConfiguration : IEntityTypeConfiguration<TransactionRatingEligibility>
{
    public void Configure(EntityTypeBuilder<TransactionRatingEligibility> builder)
    {
        builder.ToTable("TransactionRatingEligibilities", "user");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.TransactionId).HasColumnName("transaction_id").IsRequired();
        builder.Property(x => x.RaterUserId).HasColumnName("rater_user_id").IsRequired();
        builder.Property(x => x.TargetUserId).HasColumnName("target_user_id").IsRequired();
        builder.Property(x => x.OpensAt).HasColumnName("opens_at").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        builder.Property(x => x.SubmittedRatingId).HasColumnName("submitted_rating_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(x => new { x.TransactionType, x.TransactionId, x.RaterUserId, x.TargetUserId }).IsUnique();
    }
}
