using ECommerceAuction.UserService.Domain.IdentityVerifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Verification;

public sealed class BuyerVerificationProfileConfiguration
    : IEntityTypeConfiguration<BuyerVerificationProfile>
{
    public void Configure(EntityTypeBuilder<BuyerVerificationProfile> builder)
    {
        builder.ToTable("BuyerVerificationProfiles", "user");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.HasVerifiedAddress).HasColumnName("has_verified_address").HasColumnType("bit");
        builder.Property(x => x.HasVerifiedPaymentMethod).HasColumnName("has_verified_payment_method").HasColumnType("bit");

        builder.Property(x => x.AddressVerifiedAt).HasColumnName("address_verified_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.PaymentMethodVerifiedAt).HasColumnName("payment_method_verified_at").HasColumnType("datetimeoffset(3)");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetimeoffset(3)");

    }
}
