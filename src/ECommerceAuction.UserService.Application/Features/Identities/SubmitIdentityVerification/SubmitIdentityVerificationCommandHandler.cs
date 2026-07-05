using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Entities.IdentityVerification;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification
{
    public sealed class SubmitIdentityVerificationCommandHandler : ICommandHandler<SubmitIdentityVerificationCommand, SubmitIdentityVerificationResponse>
    {
        private readonly IIdentityVerificationRepository _identityVerificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityVerificationProvider _identityVerificationProvider;

        public SubmitIdentityVerificationCommandHandler(
            IIdentityVerificationRepository identityVerificationRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IIdentityVerificationProvider identityVerificationProvider)
        {
            _identityVerificationRepository = identityVerificationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _identityVerificationProvider = identityVerificationProvider;
        }
        public async Task<SubmitIdentityVerificationResponse> Handle(SubmitIdentityVerificationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken) ?? throw new NotFoundException("User not found.");

            var existingVerification = await _identityVerificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            IdentityVerification verification;

            var isFirstAttempt = existingVerification == null;

            if (existingVerification is null)
            {
                verification = new IdentityVerification(
                    userId: request.UserId,
                    identityNumber: request.IdentityNumber,
                    request.FrontImageUrl,
                    request.BackImageUrl
                );
            }
            else if (existingVerification.Status == IdentityVerificationStatus.Verified)
            {
                throw new BusinessRuleException("Identity has already been verified.");
            }
            else
            {
                // Status.Rejected -- Allow resubmission
                existingVerification.Resubmit(request.IdentityNumber, request.FrontImageUrl, request.BackImageUrl);
                verification = existingVerification;
            }
            // Call provider to verify identity
            var result = await _identityVerificationProvider.VerifyAsync(
            request.IdentityNumber, request.FrontImageUrl, request.BackImageUrl, cancellationToken);

            if (result.IsMatch)
            {
                verification.Verify(result.ConfidenceScore);
                user.ReputationProfile?.AddIdentificationVerificationPoint();
            }
            else
            {
                verification.Reject(result.FailureReason ?? "Identity verification failed.");
            }

            if (isFirstAttempt)
            {
                await _identityVerificationRepository.AddAsync(verification, cancellationToken);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (isFirstAttempt)
            {
                // Another concurrent request already inserted a verification for this user
                // (unique index on UserId) between our existence check and this save.
                throw new ConflictException("Identity verification was already submitted for this user.");
            }

            return new SubmitIdentityVerificationResponse(
                verification.Id, verification.Status, verification.ConfidenceScore, verification.RejectionReason);
        }
    }
}
