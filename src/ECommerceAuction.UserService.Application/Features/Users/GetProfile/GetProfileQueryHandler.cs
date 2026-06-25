using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Users.GetProfile;

/// <summary>
/// Handles requests for the authenticated user's profile.
/// </summary>
public sealed class GetProfileQueryHandler
    : IQueryHandler<GetProfileQuery, UserProfileResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
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

        return new UserProfileResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            IdentityNumber: user.IdentityNumber,
            Gender: user.Gender,
            Address: user.Address,
            DateOfBirth: user.DateOfBirth,
            IsEmailConfirmed: user.IsEmailConfirmed,
            IsPhoneConfirmed: user.IsPhoneConfirmed);
    }
}