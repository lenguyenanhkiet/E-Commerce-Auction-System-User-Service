using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Services;

public sealed class ReputationMutationService : IReputationMutationService
{
    private readonly IBuyerReputationRepository _buyerRepository;
    private readonly ISellerReputationRepository _sellerRepository;
    private readonly IReputationLedgerRepository _ledgerRepository;

    public ReputationMutationService(
        IBuyerReputationRepository buyerRepository,
        ISellerReputationRepository sellerRepository,
        IReputationLedgerRepository ledgerRepository)
    {
        _buyerRepository = buyerRepository;
        _sellerRepository = sellerRepository;
        _ledgerRepository = ledgerRepository;
    }

    public async Task<ReputationMutationResult> ApplyAsync(
        ReputationMutation mutation,
        CancellationToken cancellationToken = default)
    {
        Validate(mutation);
        mutation = mutation with
        {
            IdempotencyKey = mutation.IdempotencyKey.Trim()
        };

        if (await _ledgerRepository.ExistsByIdempotencyKeyAsync(
                mutation.IdempotencyKey,
                cancellationToken))
        {
            return ReputationMutationResult.DuplicateResult();
        }

        return mutation.Role switch
        {
            ReputationRoles.Buyer =>
                await ApplyBuyerAsync(mutation, cancellationToken),
            ReputationRoles.Seller =>
                await ApplySellerAsync(mutation, cancellationToken),
            _ => throw new ArgumentException(
                "Invalid reputation role.",
                nameof(mutation))
        };
    }

    public async Task<ReputationMutationResult> ReverseAsync(
        Guid originalEntryId,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (originalEntryId == Guid.Empty)
            throw new ArgumentException(
                "Original entry ID cannot be empty.",
                nameof(originalEntryId));
        if (messageId == Guid.Empty)
            throw new ArgumentException(
                "Message ID cannot be empty.",
                nameof(messageId));
        Require(idempotencyKey, "Idempotency key");
        Require(evidenceReference, "Evidence reference");

        var original = await _ledgerRepository.GetByIdAsync(
            originalEntryId,
            cancellationToken);
        if (original is null)
            throw new KeyNotFoundException(
                $"Reputation ledger entry '{originalEntryId}' was not found.");
        if (original.ReversesEntryId.HasValue)
            throw new InvalidOperationException(
                "A reversal entry cannot be reversed with this operation.");

        if (await _ledgerRepository.HasReversalAsync(
                originalEntryId,
                cancellationToken))
        {
            return ReputationMutationResult.DuplicateResult();
        }

        if (await _ledgerRepository.ExistsByIdempotencyKeyAsync(
                idempotencyKey,
                cancellationToken))
        {
            return ReputationMutationResult.DuplicateResult();
        }

        var reversalDelta = checked(-original.ScoreDelta);
        return original.Role switch
        {
            ReputationRoles.Buyer => await ReverseBuyerAsync(
                original,
                reversalDelta,
                messageId,
                correlationId,
                idempotencyKey,
                evidenceReference,
                occurredAt,
                cancellationToken),
            ReputationRoles.Seller => await ReverseSellerAsync(
                original,
                reversalDelta,
                messageId,
                correlationId,
                idempotencyKey,
                evidenceReference,
                occurredAt,
                cancellationToken),
            _ => throw new InvalidOperationException(
                $"Ledger entry has unsupported role '{original.Role}'.")
        };
    }

    private async Task<ReputationMutationResult> ApplyBuyerAsync(
        ReputationMutation mutation,
        CancellationToken cancellationToken)
    {
        var profile = await _buyerRepository.GetByUserIdAsync(
            mutation.UserId,
            cancellationToken);
        var isNew = profile is null;
        profile ??= BuyerReputationProfile.Create(
            mutation.UserId,
            mutation.OccurredAt);

        var scoreBefore = profile.ConfirmedScore;
        var trustLevelBefore = profile.TrustLevel;
        profile.ApplyConfirmedDelta(
            mutation.ScoreDelta,
            mutation.OccurredAt);

        if (isNew)
            await _buyerRepository.AddAsync(profile, cancellationToken);

        return await AddLedgerAsync(
            mutation,
            scoreBefore,
            profile.ConfirmedScore,
            trustLevelBefore,
            profile.TrustLevel,
            cancellationToken);
    }

    private async Task<ReputationMutationResult> ApplySellerAsync(
        ReputationMutation mutation,
        CancellationToken cancellationToken)
    {
        var profile = await _sellerRepository.GetByUserIdAsync(
            mutation.UserId,
            cancellationToken);
        var isNew = profile is null;
        profile ??= SellerReputationProfile.Create(
            mutation.UserId,
            mutation.OccurredAt);

        var scoreBefore = profile.ConfirmedScore;
        var trustLevelBefore = profile.TrustLevel;
        profile.ApplyConfirmedDelta(
            mutation.ScoreDelta,
            mutation.OccurredAt);

        if (isNew)
            await _sellerRepository.AddAsync(profile, cancellationToken);

        return await AddLedgerAsync(
            mutation,
            scoreBefore,
            profile.ConfirmedScore,
            trustLevelBefore,
            profile.TrustLevel,
            cancellationToken);
    }

    private async Task<ReputationMutationResult> ReverseBuyerAsync(
        ReputationLedgerEntry original,
        int reversalDelta,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var profile = await _buyerRepository.GetByUserIdAsync(
            original.UserId,
            cancellationToken);
        if (profile is null)
            throw new InvalidOperationException(
                "Buyer reputation profile was not found for reversal.");

        var scoreBefore = profile.ConfirmedScore;
        var trustLevelBefore = profile.TrustLevel;
        profile.ApplyConfirmedDelta(reversalDelta, occurredAt);

        return await AddReversalLedgerAsync(
            original,
            scoreBefore,
            profile.ConfirmedScore,
            trustLevelBefore,
            profile.TrustLevel,
            messageId,
            correlationId,
            idempotencyKey,
            evidenceReference,
            occurredAt,
            cancellationToken);
    }

    private async Task<ReputationMutationResult> ReverseSellerAsync(
        ReputationLedgerEntry original,
        int reversalDelta,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var profile = await _sellerRepository.GetByUserIdAsync(
            original.UserId,
            cancellationToken);
        if (profile is null)
            throw new InvalidOperationException(
                "Seller reputation profile was not found for reversal.");

        var scoreBefore = profile.ConfirmedScore;
        var trustLevelBefore = profile.TrustLevel;
        profile.ApplyConfirmedDelta(reversalDelta, occurredAt);

        return await AddReversalLedgerAsync(
            original,
            scoreBefore,
            profile.ConfirmedScore,
            trustLevelBefore,
            profile.TrustLevel,
            messageId,
            correlationId,
            idempotencyKey,
            evidenceReference,
            occurredAt,
            cancellationToken);
    }

    private async Task<ReputationMutationResult> AddReversalLedgerAsync(
        ReputationLedgerEntry original,
        int scoreBefore,
        int scoreAfter,
        string trustLevelBefore,
        string trustLevelAfter,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var reversal = ReputationLedgerEntry.CreateReversal(
            original,
            scoreBefore,
            messageId,
            correlationId,
            idempotencyKey,
            evidenceReference,
            occurredAt,
            occurredAt);
        await _ledgerRepository.AddAsync(reversal, cancellationToken);

        return ReputationMutationResult.AppliedResult(
            reversal.Id,
            reversal.Role,
            scoreBefore,
            scoreAfter,
            trustLevelBefore,
            trustLevelAfter);
    }

    private async Task<ReputationMutationResult> AddLedgerAsync(
        ReputationMutation mutation,
        int scoreBefore,
        int scoreAfter,
        string trustLevelBefore,
        string trustLevelAfter,
        CancellationToken cancellationToken)
    {
        var entry = ReputationLedgerEntry.Create(
            mutation,
            scoreBefore,
            scoreAfter,
            mutation.OccurredAt);
        await _ledgerRepository.AddAsync(entry, cancellationToken);

        return ReputationMutationResult.AppliedResult(
            entry.Id,
            entry.Role,
            scoreBefore,
            scoreAfter,
            trustLevelBefore,
            trustLevelAfter);
    }

    private static void Validate(ReputationMutation mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        if (mutation.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(mutation));
        if (!ReputationRoles.IsValid(mutation.Role))
            throw new ArgumentException("Invalid reputation role.", nameof(mutation));
        if (!ReputationReasonCatalog.IsKnown(mutation.ReasonCode))
            throw new ArgumentException("Invalid reputation reason code.", nameof(mutation));
        if (!ReasonMatchesRole(mutation.Role, mutation.ReasonCode))
            throw new ArgumentException(
                "Reputation reason code does not match the selected role.",
                nameof(mutation));
        if (mutation.ScoreDelta == 0)
            throw new ArgumentOutOfRangeException(nameof(mutation), "Score delta cannot be zero.");
        if (mutation.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(mutation));
        Require(mutation.SourceService, "Source service");
        Require(mutation.SourceType, "Source type");
        Require(mutation.SourceId, "Source ID");
        Require(mutation.IdempotencyKey, "Idempotency key");
        Require(mutation.RuleVersion, "Rule version");
    }

    private static void Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} is required.");
    }

    private static bool ReasonMatchesRole(string role, string reasonCode) =>
        role switch
        {
            ReputationRoles.Buyer => reasonCode.StartsWith(
                "buyer.",
                StringComparison.Ordinal),
            ReputationRoles.Seller => reasonCode.StartsWith(
                "seller.",
                StringComparison.Ordinal),
            _ => false
        };
}
