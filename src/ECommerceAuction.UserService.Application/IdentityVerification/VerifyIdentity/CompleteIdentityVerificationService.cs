using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.IdentityVerifications.VerifyIdentity;

public sealed class CompleteIdentityVerificationService
{
    private readonly IReputationAwardService _reputationAwardService;

    public CompleteIdentityVerificationService(
        IReputationAwardService reputationAwardService)
    {
        _reputationAwardService = reputationAwardService;
    }

    public async Task CompleteAsync(
        Guid userId,
        string verificationReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verificationReference))
        {
            throw new ArgumentException(
                "Identity verification reference is required.",
                nameof(verificationReference));
        }

        // User owns the identity-verification flag; IdentityVerification owns
        // the review details. The ledger idempotency key prevents double awards.
        await _reputationAwardService.AwardConfirmedAsync(
            userId: userId,
            entryType:
                ReputationEntryTypes.ProfileVerification,
            reason:
                ReputationReasons.IdentityVerified,
            points:
                BuyerReputationPoints.IdentityVerified,
            sourceType:
                "IDENTITY_VERIFICATION",
            sourceId:
                verificationReference,
            idempotencyKey:
                $"user:{userId}:identity-verified:v1",
            occurredAt:
                occurredAt,
            cancellationToken:
                cancellationToken);

    }
}
