using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.VerifyPhone;

public sealed class CompletePhoneVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IReputationAwardService _reputationAwardService;

    public CompletePhoneVerificationService(
        IBuyerVerificationRepository buyerVerificationRepository,
        IReputationAwardService reputationAwardService)
    {
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
    }

    public async Task CompleteAsync(
        Guid userId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var profile = await _buyerVerificationRepository.GetByUserIdAsync(
            userId,
            cancellationToken);
        if (profile is null)
        {
            profile = BuyerVerificationProfile.Create(userId, occurredAt);
            await _buyerVerificationRepository.AddAsync(profile, cancellationToken);
        }

        if (!profile.VerifyPhone(occurredAt))
        {
            return;
        }

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
