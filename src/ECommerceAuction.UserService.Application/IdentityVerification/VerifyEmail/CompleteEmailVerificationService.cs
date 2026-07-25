using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.IdentityVerifications.VerifyEmail;

public sealed class CompleteEmailVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;

    private readonly IReputationAwardService _reputationAwardService;

    private readonly IUnitOfWork _unitOfWork;

    public CompleteEmailVerificationService(IBuyerVerificationRepository buyerVerificationRepository, IReputationAwardService reputationAwardService, IUnitOfWork unitOfWork)
    {
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
        _unitOfWork = unitOfWork;
    }

    public async Task CompleteAsync(
        Guid userId,
        DateTime occurredAt,
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

        var isFirstVerification =
            verificationProfile.VerifyEmail(occurredAt);

        if (!isFirstVerification)
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

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}