using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateProfile;

/// <summary>
/// Updates the current user's phone number, address, and optionally requests
/// an email change.
/// </summary>
public sealed record UpdateProfileCommand(
    string PhoneNumber,
    string? NewEmail)
    : ICommand<UpdateProfileResponse>;

public sealed record UpdateProfileResponse(
    bool IsUpdated,
    bool EmailVerificationSent,
    string Message);

/// <summary>
/// Integration event published for Notification Service.
/// This contract should later be moved to Nexus.Contracts.
/// </summary>
public sealed record ProfileEmailChangeRequested(
    Guid UserId,
    string CurrentEmail,
    string NewEmail,
    string FullName,
    string VerificationToken,
    DateTimeOffset ExpiresAtUtc,
    Guid CorrelationId);