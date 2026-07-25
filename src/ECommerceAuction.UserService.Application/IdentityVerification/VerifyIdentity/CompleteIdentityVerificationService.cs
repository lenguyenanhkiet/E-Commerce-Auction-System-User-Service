using ECommerceAuction.UserService.Application.Abstractions.Persistence;
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
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;

    private readonly IReputationAwardService _reputationAwardService;

    private readonly IUnitOfWork _unitOfWork;

    public CompleteIdentityVerificationService(IBuyerVerificationRepository buyerVerificationRepository, IReputationAwardService reputationAwardService, IUnitOfWork unitOfWork)
    {
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
        _unitOfWork = unitOfWork;
    }

    public async Task CompleteAsync(
        Guid userId,
        string verificationReference,
        DateTime occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verificationReference))
        {
            throw new ArgumentException(
                "Identity verification reference is required.",
                nameof(verificationReference));
        }

        var verificationProfile =
            await _buyerVerificationRepository
                .GetByUserIdAsync(
                    userId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Buyer verification profile was not found.");

        var isFirstVerification =
            verificationProfile.VerifyIdentity(occurredAt);

        if (!isFirstVerification)
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

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}