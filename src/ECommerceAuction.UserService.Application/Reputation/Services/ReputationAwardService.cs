using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Reputation.Services;

/// <summary>
/// Coordinates creation of a confirmed ledger entry and updates
/// the corresponding buyer reputation summary.
///
/// SaveChanges is intentionally not called here so the caller can commit
/// verification state, ledger and profile in one database transaction.
/// </summary>

public sealed class ReputationAwardService : IReputationAwardService
{
    private readonly IBuyerReputationRepository _buyerReputationRepository;
    private readonly IReputationLedgerRepository _reputationLedgerRepository;

    public ReputationAwardService(IBuyerReputationRepository buyerReputationRepository, IReputationLedgerRepository reputationLedgerRepository)
    {
        _buyerReputationRepository = buyerReputationRepository;
        _reputationLedgerRepository = reputationLedgerRepository;
    }

    public async Task<bool> AwardConfirmedAsync(Guid userId, string entryType, string reason, int points, string sourceType, string sourceId, string idempotencyKey, DateTime occurredAt, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Awarded reputation points must be positive.");
        }

        var alreadyProcessed =
            await _reputationLedgerRepository
                .ExistsByIdempotencyKeyAsync(
                    idempotencyKey,
                    cancellationToken);

        if (alreadyProcessed)
        {
            return false;
        }

        var ledgerEntry =
            ReputationLedgerEntry.CreateConfirmed(
                userId: userId,
                entryType: entryType,
                reason: reason,
                points: points,
                sourceService: "user-service",
                sourceType: sourceType,
                sourceId: sourceId,
                idempotencyKey: idempotencyKey,
                occurredAt: occurredAt);

        await _reputationLedgerRepository.AddAsync(
            ledgerEntry,
            cancellationToken);

        var profile =
            await _buyerReputationRepository
                .GetByUserIdAsync(
                    userId,
                    cancellationToken);

        if (profile is null)
        {
            profile = BuyerReputationProfile.Create(
                userId,
                occurredAt);

            await _buyerReputationRepository.AddAsync(
                profile,
                cancellationToken);
        }

        profile.ApplyConfirmedPoints(
            points,
            occurredAt);

        return true;
    }
}