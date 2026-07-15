using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit.NewIdProviders;

namespace ECommerceAuction.UserService.Application.Features.Users.GetProfile;

/// <summary>
/// Handles requests for the authenticated user's profile.
/// </summary>
public sealed class GetProfileQueryHandler
    : IQueryHandler<GetProfileQuery, UserProfileResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IIdentityVerificationRepository _identityVerificationRepository;
    private readonly IAddressRepository _addressRepository;

    public GetProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserOAuthRepository userOAuthRepository,
        IIdentityVerificationRepository identityVerificationRepository,
        IAddressRepository addressRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userOAuthRepository = userOAuthRepository;
        _identityVerificationRepository = identityVerificationRepository;
        _addressRepository = addressRepository;
    }

    public async Task<UserProfileResponse> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        // UserId is read from the JWT NameIdentifier claim.
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException(
                "User information not found in JWT.");

        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User information not found.");
        }
        var roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);
        var privileges = await _userOAuthRepository.GetActivePrivilegeCodesAsync(user.Id, cancellationToken);
        var identityVerification = await _identityVerificationRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var addresses = await _addressRepository.GetUserAddressesAsync(user.Id, cancellationToken);
        return new UserProfileResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            IdentityNumber: identityVerification?.IdentityNumber,
            Gender: user.Gender,
            AvatarUrl: user.AvatarUrl,
            AddressList: addresses.Select(a => new AddressResponse(
                a.RecipientName,
                a.RecipientPhone,
                a.Street,
                a.Province,
                a.Ward,
                a.Type,
                a.IsDefault)).ToList(),
            DateOfBirth: user.DateOfBirth,
            IsEmailConfirmed: user.IsEmailConfirmed,
            IsPhoneConfirmed: user.IsPhoneConfirmed,
            Reputation: user.ReputationProfile is null
        ? new UserReputationResponse(0, "Silver")
        : new UserReputationResponse(user.ReputationProfile.Score, user.ReputationProfile.TrustLevel),
            Roles: roles,
            Privileges: privileges);
    }
}