using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Users.GetProfile;

/// <summary>
/// Gets the profile of the currently authenticated user.
/// </summary>
public sealed record GetProfileQuery : IQuery<UserProfileResponse>;

/// <summary>
/// Profile data returned to the current user.
/// </summary>
public sealed record UserProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? IdentityNumber,
    string? Gender,
    string? Address,
    DateOnly? DateOfBirth,
    bool IsEmailConfirmed,
    bool IsPhoneConfirmed,
    UserReputationResponse Reputation,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Privileges);

public sealed record UserReputationResponse(
    int Score,
    string Trust_level
);

