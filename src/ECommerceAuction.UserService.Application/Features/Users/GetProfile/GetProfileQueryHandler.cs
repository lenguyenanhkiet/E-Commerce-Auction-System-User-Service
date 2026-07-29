using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit.NewIdProviders;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;

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
    private readonly IBuyerReputationRepository _buyerReputationRepository;
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;

    public GetProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUserOAuthRepository userOAuthRepository,
        IIdentityVerificationRepository identityVerificationRepository,
        IAddressRepository addressRepository,
        IBuyerReputationRepository buyerReputationRepository,
        IBuyerVerificationRepository buyerVerificationRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _userOAuthRepository = userOAuthRepository;
        _identityVerificationRepository = identityVerificationRepository;
        _addressRepository = addressRepository;
        _buyerReputationRepository = buyerReputationRepository;
        _buyerVerificationRepository = buyerVerificationRepository;
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
        var reputation = await _buyerReputationRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);
        var buyerVerification = await _buyerVerificationRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        var isEmailVerified = buyerVerification?.IsEmailVerified ?? false;
        var isPhoneVerified = buyerVerification?.IsPhoneVerified ?? false;
        var isIdentityVerified = buyerVerification?.IsIdentityVerified ?? false;
        var hasVerifiedAddress = buyerVerification?.HasVerifiedAddress ?? false;
        var hasVerifiedPaymentMethod = buyerVerification?.HasVerifiedPaymentMethod ?? false;

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
            Verification: new UserVerificationResponse(
                Email: new VerificationStateResponse(
                    isEmailVerified,
                    buyerVerification?.EmailVerifiedAt),
                Phone: new VerificationStateResponse(
                    isPhoneVerified,
                    buyerVerification?.PhoneVerifiedAt),
                Identity: new VerificationStateResponse(
                    isIdentityVerified,
                    buyerVerification?.IdentityVerifiedAt),
                Address: new VerificationStateResponse(
                    hasVerifiedAddress,
                    buyerVerification?.AddressVerifiedAt),
                PaymentMethod: new VerificationStateResponse(
                    hasVerifiedPaymentMethod,
                    buyerVerification?.PaymentMethodVerifiedAt),
                IsFullyVerified: buyerVerification?.IsFullyVerified ?? false),
            AuthProvider: user.AuthProvider,
            Reputation: reputation is null
                ? new UserReputationResponse(0, BuyerTrustLevels.Basic)
                : new UserReputationResponse(reputation.ConfirmedScore, reputation.TrustLevel),
            Roles: roles,
            Privileges: privileges);
    }
}
