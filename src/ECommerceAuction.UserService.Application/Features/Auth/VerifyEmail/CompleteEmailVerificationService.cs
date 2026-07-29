using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;

public sealed class CompleteEmailVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;

    private readonly IReputationAwardService _reputationAwardService;

    public CompleteEmailVerificationService(
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
        var verificationProfile =
            await _buyerVerificationRepository
                .GetByUserIdAsync(
                    userId,
                    cancellationToken);

        if (verificationProfile is null)
        {
            verificationProfile =
                BuyerVerificationProfile.Create(
                    userId,
                    occurredAt);

            await _buyerVerificationRepository.AddAsync(
                verificationProfile,
                cancellationToken);
        }

        if (!verificationProfile.VerifyEmail(occurredAt))
        {
            return;
        }

        await _reputationAwardService.AwardConfirmedAsync(
            userId: userId,
            entryType:
                ReputationEntryTypes.ProfileVerification,
            reason:
                ReputationReasons.EmailVerified,
            points:
                BuyerReputationPoints.EmailVerified,
            sourceType:
                "USER_EMAIL",
            sourceId:
                userId.ToString(),
            idempotencyKey:
                $"user:{userId}:email-verified:v1",
            occurredAt:
                occurredAt,
            cancellationToken:
                cancellationToken);

    }
}
