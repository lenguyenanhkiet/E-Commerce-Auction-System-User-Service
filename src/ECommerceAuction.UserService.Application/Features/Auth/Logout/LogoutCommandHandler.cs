using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.Logout;

/// <summary>
/// Handles user logout by revoking the current access token, revoking the refresh session, and writing an audit log.
/// </summary>
public sealed class LogoutCommandHandler
    : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        ICurrentUserService currentUserService,
        ITokenRevocationService tokenRevocationService,
        IRefreshTokenService refreshTokenService,
        IUserOAuthRepository userOAuthRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _tokenRevocationService = tokenRevocationService;
        _refreshTokenService = refreshTokenService;
        _userOAuthRepository = userOAuthRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Executes logout for the authenticated user and returns a confirmation response.
    /// </summary>
    public async Task<LogoutResponse> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // ECA-10 Logout: reject logout if request does not contain a valid authenticated user.
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException("User must be authenticated before logout.");
        }

        // ECA-10 Logout: revoke the current access token so it cannot be reused after logout.
        await _tokenRevocationService.RevokeCurrentAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var refreshTokenHash = _refreshTokenService.HashToken(request.RefreshToken);
            var session = await _userOAuthRepository.GetActiveSessionByRefreshTokenHashAsync(
                refreshTokenHash,
                cancellationToken);

            session?.Revoke();
        }

        await _userOAuthRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: _currentUserService.UserId,
                targetUserId: _currentUserService.UserId,
                action: UserAuditActions.Logout,
                entityType: nameof(User),
                entityId: _currentUserService.UserId.Value.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LogoutResponse(
            Success: true,
            Message: "User has logged out successfully.",
            LoggedOutAt: DateTimeOffset.UtcNow);
    }
}
