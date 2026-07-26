using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Identities.VerifyIdentity;
using ECommerceAuction.UserService.Application.Services.IdentityMatching;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly IdentityMatchingOptions _options;
        private readonly CompleteIdentityVerificationService _completeIdentityVerification;

        public SubmitIdentityVerificationCommandHandler(
            IIdentityVerificationRepository identityVerificationRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IIdentityVerificationProvider identityVerificationProvider,
            IOptions<IdentityMatchingOptions> options,
            CompleteIdentityVerificationService completeIdentityVerification)
        {
            _identityVerificationRepository = identityVerificationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _identityVerificationProvider = identityVerificationProvider;
            _options = options.Value;
            _completeIdentityVerification = completeIdentityVerification;
        }

        public async Task<SubmitIdentityVerificationResponse> Handle(SubmitIdentityVerificationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken) ?? throw new NotFoundException("User not found.");
            var identityJustVerified = false;

            // Upload keys are "{service}/{imageType}/{ownerId}/...", and the caller hands them to us
            // straight from the request. Without this, anyone could submit somebody else's uploaded
            // card, let it read cleanly, declare the details it shows, and be verified as them.
            EnsureImageBelongsToCaller(request.FrontImageKey, request.UserId);
            EnsureImageBelongsToCaller(request.BackImageKey, request.UserId);

            var existingVerification = await _identityVerificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            IdentityVerification verification;

            var isFirstAttempt = existingVerification == null;

            // Captured before Resubmit overwrites them, so the caller can drop the replaced files.
            var previousImageKeys = new List<string>();

            if (existingVerification is null)
            {
                verification = new IdentityVerification(
                    userId: request.UserId,
                    fullName: request.FullName,
                    gender: request.Gender,
                    dateOfBirth: request.DateOfBirth,
                    identityNumber: request.IdentityNumber,
                    issueDate: request.IssueDate,
                    expiryDate: request.ExpiryDate,
                    issuePlace: request.IssuePlace,
                    permanentAddress: request.PermanentAddress,
                    request.FrontImageKey,
                    request.BackImageKey
                );
            }
            else if (existingVerification.Status == IdentityVerificationState.Verified)
            {
                throw new BusinessRuleException("Identity has already been verified.");
            }
            else
            {
                // Status.Rejected -- Allow resubmission
                CollectReplacedKey(previousImageKeys, existingVerification.IdentityFrontImageKey, request.FrontImageKey);
                CollectReplacedKey(previousImageKeys, existingVerification.IdentityBackImageKey, request.BackImageKey);

                existingVerification.Resubmit(request.FullName, request.Gender, request.DateOfBirth, request.IdentityNumber, request.IssueDate, request.ExpiryDate, request.IssuePlace, request.PermanentAddress, request.FrontImageKey, request.BackImageKey);
                verification = existingVerification;
            }
            // Read the card, then check it against what the user declared. Reading the image is not
            // the same as agreeing with it: a legible card belonging to somebody else must not pass.
            var extractionResult = await _identityVerificationProvider.ExtractAsync(
                request.FrontImageKey, request.BackImageKey, cancellationToken);

            if (!extractionResult.Success || extractionResult.Extraction is null)
            {
                verification.Reject(extractionResult.FailureReason ?? "Identity verification failed.");
            }
            else
            {
                var declared = new IdentityDeclaration(
                    request.FullName,
                    request.Gender,
                    request.DateOfBirth,
                    request.IdentityNumber);

                var outcome = IdentityMatcher.Match(
                    declared,
                    extractionResult.Extraction,
                    _options.MinimumConfidence);

                if (outcome.IsMatch)
                {
                    verification.Verify(extractionResult.Extraction.Confidence);
                    user.VerifyIdentity(
                        extractionResult.Extraction.FullName ?? request.FullName,
                        extractionResult.Extraction.Gender ?? request.Gender,
                        extractionResult.Extraction.DateOfBirth ?? request.DateOfBirth);
                    identityJustVerified = true;
                }
                else
                {
                    verification.Reject(outcome.FailureReason ?? "Identity verification failed.");
                }
            }

            if (isFirstAttempt)
            {
                await _identityVerificationRepository.AddAsync(verification, cancellationToken);
            }

            // Award +5 through the reputation ledger once the identity is verified. Idempotent via
            // the ledger idempotency key, so a retried KYC callback never awards twice.
            if (identityJustVerified)
            {
                await _completeIdentityVerification.CompleteAsync(
                    user.Id, verification.Id.ToString(), DateTimeOffset.UtcNow, cancellationToken);
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
                verification.Id, verification.Status, verification.ConfidenceScore, verification.RejectionReason,
                previousImageKeys);
        }

        /// <summary>
        /// Records the old key only when the resubmission actually replaced it — re-sending the same
        /// key must not queue the file the record still points at for deletion.
        /// </summary>
        private static void CollectReplacedKey(List<string> replaced, string oldKey, string newKey)
        {
            if (!string.IsNullOrWhiteSpace(oldKey) &&
                !string.Equals(oldKey, newKey, StringComparison.OrdinalIgnoreCase))
            {
                replaced.Add(oldKey);
            }
        }

        /// <summary>
        /// Mirrors the ownership guard the delete endpoint already applies to the same key space.
        /// The message stays vague on purpose: whether a given key exists is not the caller's business.
        /// </summary>
        private static void EnsureImageBelongsToCaller(string key, Guid userId)
        {
            if (!key.Contains($"/{userId}/", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("The uploaded identity images are not valid for this account.");
            }
        }
    }
}
