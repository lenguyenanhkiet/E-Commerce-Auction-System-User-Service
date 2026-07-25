using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.IdentityVerification.VerifyPhone;

public sealed class CompletePhoneVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;

    private readonly IReputationAwardService _reputationAwardService;

    private readonly IUnitOfWork _unitOfWork;

    public CompletePhoneVerificationService(IBuyerVerificationRepository buyerVerificationRepository, IReputationAwardService reputationAwardService, IUnitOfWork unitOfWork)
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
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Buyer verification profile was not found.");

        var isFirstVerification =
            verificationProfile.VerifyPhone(occurredAt);

        if (!isFirstVerification)
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

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}