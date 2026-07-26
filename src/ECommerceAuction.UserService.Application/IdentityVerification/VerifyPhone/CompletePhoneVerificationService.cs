using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.IdentityVerifications.VerifyPhone;

public sealed class CompletePhoneVerificationService
{
    private readonly IReputationAwardService _reputationAwardService;

    public CompletePhoneVerificationService(
        IReputationAwardService reputationAwardService)
    {
        _reputationAwardService = reputationAwardService;
    }

    public async Task CompleteAsync(
        Guid userId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        // The User aggregate owns phone verification state. The ledger's
        // idempotency key prevents duplicate reputation awards.
        await _reputationAwardService.AwardConfirmedAsync(
            userId: userId,
            entryType:
                ReputationEntryTypes.ProfileVerification,
            reason:
                ReputationReasons.PhoneVerified,
            points:
                BuyerReputationPoints.PhoneVerified,
            sourceType:
                "USER_PHONE",
            sourceId:
                userId.ToString(),
            idempotencyKey:
                $"user:{userId}:phone-verified:v1",
            occurredAt:
                occurredAt,
            cancellationToken:
                cancellationToken);

    }
}
