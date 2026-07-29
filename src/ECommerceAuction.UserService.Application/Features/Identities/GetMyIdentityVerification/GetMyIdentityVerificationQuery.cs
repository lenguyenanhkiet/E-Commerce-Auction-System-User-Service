using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.GetMyIdentityVerification;

public sealed record GetMyIdentityVerificationQuery(Guid UserId) : IQuery<IdentityVerificationResponse?>;
public sealed record IdentityVerificationResponse(
    Guid Id,
    string FullName,
    string Gender,
    DateOnly DateOfBirth,
    string IdentityNumber,
    DateOnly IssueDate,
    DateOnly ExpiryDate,
    string IssuePlace,
    string PermanentAddress,

    string IdentityFrontImageKey,
    string IdentityBackImageKey,

    string Status,
    string? RejectionReason,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? VerifiedAt

);