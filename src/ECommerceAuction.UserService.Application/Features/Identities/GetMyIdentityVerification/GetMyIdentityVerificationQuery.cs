using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.GetMyIdentityVerification
{
    public sealed record GetMyIdentityVerificationQuery(Guid UserId) : IQuery<IdentityVerificationResponse?>;
    public sealed record IdentityVerificationResponse(
    Guid Id,
    string Status,
    decimal? ConfidenceScore,
    string? RejectionReason,
    DateTime SubmittedAt,
    DateTime? VerifiedAt);

}
