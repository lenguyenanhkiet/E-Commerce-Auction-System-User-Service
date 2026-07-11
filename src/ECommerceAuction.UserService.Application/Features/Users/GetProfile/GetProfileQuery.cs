using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Domain.Entities.Users;

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
    IReadOnlyList<AddressResponse> AddressList,
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
public sealed record AddressResponse(
    string RecipientName,
    string RecipientPhone,
    string Street,
    string Province,
    string City,
    string Ward,
    string Type,
    bool IsDefault
    );

