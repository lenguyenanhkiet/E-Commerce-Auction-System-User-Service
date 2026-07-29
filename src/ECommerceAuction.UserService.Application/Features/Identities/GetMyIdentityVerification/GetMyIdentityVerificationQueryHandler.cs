using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.GetMyIdentityVerification
{
    public sealed class GetMyIdentityVerificationQueryHandler
    : IQueryHandler<GetMyIdentityVerificationQuery, IdentityVerificationResponse?>
    {
        private readonly IIdentityVerificationRepository _identityVerificationRepository;

        public GetMyIdentityVerificationQueryHandler(IIdentityVerificationRepository identityVerificationRepository)
        {
            _identityVerificationRepository = identityVerificationRepository;
        }

        public async Task<IdentityVerificationResponse?> Handle(GetMyIdentityVerificationQuery request, CancellationToken cancellationToken)
        {
            var verification = await _identityVerificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            return verification is null
            ? null
            : new IdentityVerificationResponse(
                verification.Id,
                verification.FullName,
                verification.Gender,
                verification.DateOfBirth,
                verification.IdentityNumber,
                verification.IssueDate,
                verification.ExpiryDate,
                verification.IssuePlace,
                verification.PermanentAddress,
                verification.IdentityFrontImageKey,
                verification.IdentityBackImageKey,
                verification.Status,
                verification.RejectionReason,
                verification.SubmittedAt,
                verification.VerifiedAt);
        }
    }
}