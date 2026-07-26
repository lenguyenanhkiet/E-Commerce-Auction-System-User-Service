using ECommerceAuction.UserService.Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.PaymentMethods;

public sealed class BankAccountVerificationConfiguration
    : IEntityTypeConfiguration<BankAccountVerification>
{
    public void Configure(EntityTypeBuilder<BankAccountVerification> builder)
    {
        builder.ToTable("BankAccountVerifications", "user");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProviderReference).HasColumnName("provider_reference").HasMaxLength(200).IsRequired();
        builder.Property(x => x.BankCode).HasColumnName("bank_code").HasMaxLength(30).IsRequired();
        builder.Property(x => x.MaskedAccountNumber).HasColumnName("masked_account_number").HasMaxLength(64).IsRequired();
        builder.Property(x => x.AccountFingerprint).HasColumnName("account_fingerprint").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ExpectedAccountHolderName).HasColumnName("expected_account_holder_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.VerifiedAccountHolderName).HasColumnName("verified_account_holder_name").HasMaxLength(200);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.FailureCode).HasColumnName("failure_code").HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.VerifiedAt).HasColumnName("verified_at");
        builder.Property(x => x.RejectedAt).HasColumnName("rejected_at");
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.Provider, x.ProviderReference }).IsUnique();
        builder.HasIndex(x => x.AccountFingerprint);
        builder.HasIndex(x => x.UserId);
    }
}
