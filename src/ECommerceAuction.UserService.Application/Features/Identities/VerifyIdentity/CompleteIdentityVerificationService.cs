using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.VerifyIdentity;

public sealed class CompleteIdentityVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IReputationAwardService _reputationAwardService;

    public CompleteIdentityVerificationService(
        IBuyerVerificationRepository buyerVerificationRepository,
        IReputationAwardService reputationAwardService)
    {
        _buyerVerificationRepository = buyerVerificationRepository;
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

        var profile = await _buyerVerificationRepository.GetByUserIdAsync(
            userId,
            cancellationToken);
        if (profile is null)
        {
            profile = BuyerVerificationProfile.Create(userId, occurredAt);
            await _buyerVerificationRepository.AddAsync(profile, cancellationToken);
        }

        if (!profile.VerifyIdentity(occurredAt))
        {
            return;
        }

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
