using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAuction.UserService.Persistence.Configurations.Reputation;

public sealed class ReputationLedgerEntryConfiguration
    : IEntityTypeConfiguration<ReputationLedgerEntry>
{
    public void Configure(EntityTypeBuilder<ReputationLedgerEntry> builder)
    {
        builder.ToTable("ReputationLedgerEntries", "user", table =>
        {
            table.HasCheckConstraint(
                "CK_ReputationLedgerEntries_points_non_zero",
                "[points] <> 0");

            table.HasCheckConstraint(
                "CK_ReputationLedgerEntries_status",
                "[status] IN ('PENDING','CONFIRMED','REVERSED','CANCELLED')");

            table.HasCheckConstraint(
                "CK_ReputationLedgerEntries_entry_type",
                "[entry_type] IN ('PROFILE_VERIFICATION','ECOMMERCE_TRANSACTION','AUCTION_TRANSACTION','AUCTION_BONUS','REVIEW','PENALTY','REVERSAL')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(x => x.EntryType)
            .HasColumnName("entry_type")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("nvarchar(60)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("nvarchar(20)")
            .IsRequired();

        builder.Property(x => x.Points)
            .HasColumnName("points")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.SourceService)
            .HasColumnName("source_service")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasColumnName("source_type")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.SourceId)
            .HasColumnName("source_id")
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.Property(x => x.RuleVersion)
            .HasColumnName("rule_version")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.ReversalEntryId)
            .HasColumnName("reversal_entry_id")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.ConfirmAfter).HasColumnName("confirm_after").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.ReversedAt).HasColumnName("reversed_at").HasColumnType("datetimeoffset(3)");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetimeoffset(3)");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetimeoffset(3)");

        // Idempotency: the same reward/penalty can never be recorded twice.
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();

        // Fast per-user history queries filtered by status.
        builder.HasIndex(x => new { x.UserId, x.Status, x.CreatedAt });

        // Trace a ledger entry back to the business event that produced it.
        builder.HasIndex(x => new { x.SourceService, x.SourceType, x.SourceId });
    }
}
