using System.Security.Cryptography;
using System.Text;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Services;

/// <summary>
/// Compatibility adapter for existing profile-verification callers.
/// All score changes are delegated to IReputationMutationService.
/// </summary>
public sealed class ReputationAwardService : IReputationAwardService
{
    private readonly IReputationMutationService _mutationService;

    public ReputationAwardService(IReputationMutationService mutationService)
    {
        _mutationService = mutationService;
    }

    public async Task<bool> AwardConfirmedAsync(
        Guid userId,
        string entryType,
        string reason,
        int points,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        if (points <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Awarded reputation points must be positive.");
        if (!ReputationEntryTypes.IsValid(entryType))
            throw new ArgumentException(
                "Invalid legacy reputation entry type.",
                nameof(entryType));

        idempotencyKey = idempotencyKey?.Trim()
            ?? throw new ArgumentNullException(nameof(idempotencyKey));
        var mutation = new ReputationMutation(
            userId,
            ReputationRoles.Buyer,
            MapLegacyReason(reason),
            points,
            "user-service",
            sourceType,
            sourceId,
            idempotencyKey,
            "REPUTATION_V1",
            CreateDeterministicMessageId(idempotencyKey),
            null,
            null,
            occurredAt);

        var result = await _mutationService.ApplyAsync(
            mutation,
            cancellationToken);
        return result.Applied;
    }

    private static string MapLegacyReason(string reason) => reason switch
    {
        ReputationReasons.EmailVerified =>
            ReputationReasonCatalog.BuyerProfileEmailVerified,
        ReputationReasons.PhoneVerified =>
            ReputationReasonCatalog.BuyerProfilePhoneVerified,
        ReputationReasons.IdentityVerified =>
            ReputationReasonCatalog.BuyerProfileIdentityVerified,
        ReputationReasons.AddressVerified =>
            ReputationReasonCatalog.BuyerProfileAddressVerified,
        ReputationReasons.PaymentMethodVerified =>
            ReputationReasonCatalog.BuyerProfilePaymentMethodLinked,
        _ => throw new ArgumentException(
            "Unsupported legacy reputation reason.",
            nameof(reason))
    };

    private static Guid CreateDeterministicMessageId(string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(idempotencyKey));
        return new Guid(bytes.AsSpan(0, 16));
    }
}
