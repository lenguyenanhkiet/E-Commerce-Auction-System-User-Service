using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.GetUserById;

/// <summary>
/// Handles Admin single-user detail queries.
/// </summary>
public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, AdminUserDetailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityVerificationRepository _identityVerificationRepository;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IIdentityVerificationRepository identityVerificationRepository)
    {
        _userRepository = userRepository;
        _identityVerificationRepository = identityVerificationRepository;
    }

    public async Task<AdminUserDetailResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        // GetByIdAsync already excludes soft-deleted users, satisfying the
        // "only display if the account exists and has not been deleted" rule.
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} was not found.");

        var identityVerification =
            await _identityVerificationRepository.GetByUserIdAsync(user.Id, cancellationToken);

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        return new AdminUserDetailResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            IdentityNumber: identityVerification?.IdentityNumber,
            Gender: user.Gender,
            Address: user.GetDefaultAddress()?.Street,
            DateOfBirth: user.DateOfBirth,
            Status: user.Status,
            IsEmailConfirmed: user.IsEmailConfirmed,
            IsPhoneConfirmed: user.IsPhoneConfirmed,
            Roles: roles,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt);
    }
}
