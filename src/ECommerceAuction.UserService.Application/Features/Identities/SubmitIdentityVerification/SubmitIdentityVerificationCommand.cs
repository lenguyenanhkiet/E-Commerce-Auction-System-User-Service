using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification;

public record SubmitIdentityVerificationCommand
    (
        Guid UserId,
        string FullName,
        string Gender,
        DateOnly DateOfBirth,
        string IdentityNumber,
        DateOnly IssueDate,
        DateOnly ExpiryDate,
        string IssuePlace,
        string PermanentAddress,
        string FrontImageKey,
        string BackImageKey
    ) : ICommand<SubmitIdentityVerificationResponse>;

/// <summary>
/// <paramref name="PreviousImageKeys"/> are the images this submission replaced (empty on a first
/// attempt). The API layer deletes them from storage — this layer has no storage dependency, the
/// same split the avatar flow uses.
/// </summary>
public record SubmitIdentityVerificationResponse
    (
        Guid Id,
        string status,
        decimal? ConfidenceScore,
        string? RejectionReason,
        IReadOnlyList<string> PreviousImageKeys
    );