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
            table.HasCheckConstraint(
                "CK_ReputationLedgerEntries_score_delta_non_zero",
                "[score_delta] <> 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(20)
            .IsUnicode(false).IsRequired();
        builder.Property(x => x.ReasonCode).HasColumnName("reason_code")
            .HasMaxLength(150).IsUnicode(false).IsRequired();
        builder.Property(x => x.ScoreDelta).HasColumnName("score_delta").IsRequired();
        builder.Property(x => x.ScoreBefore).HasColumnName("score_before").IsRequired();
        builder.Property(x => x.ScoreAfter).HasColumnName("score_after").IsRequired();
        builder.Property(x => x.SourceService).HasColumnName("source_service")
            .HasMaxLength(80).IsUnicode(false).IsRequired();
        builder.Property(x => x.SourceType).HasColumnName("source_type")
            .HasMaxLength(80).IsUnicode(false).IsRequired();
        builder.Property(x => x.SourceId).HasColumnName("source_id")
            .HasMaxLength(200).IsUnicode(false).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key")
            .HasMaxLength(450).IsUnicode(false).IsRequired();
        builder.Property(x => x.RuleVersion).HasColumnName("rule_version")
            .HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.Property(x => x.MessageId).HasColumnName("message_id").IsRequired();
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        builder.Property(x => x.ReversesEntryId).HasColumnName("reverses_entry_id");
        builder.Property(x => x.EvidenceReference).HasColumnName("evidence_reference")
            .HasMaxLength(500);
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Role, x.OccurredAt });
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ReversesEntryId)
            .IsUnique()
            .HasFilter("[reverses_entry_id] IS NOT NULL");
    }
}
