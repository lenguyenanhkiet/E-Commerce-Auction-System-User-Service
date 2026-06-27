using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.Common;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.RefreshToken;

/// <summary>
/// Handles refresh token rotation by validating an active session and issuing a new token pair.
/// </summary>
public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserOAuthRepository userOAuthRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork)
    {
        _userOAuthRepository = userOAuthRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Validates the submitted refresh token, rotates the session token, and returns fresh authentication data.
    /// </summary>
    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required.");
        }

        var refreshTokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var session = await _userOAuthRepository.GetActiveSessionByRefreshTokenHashAsync(
            refreshTokenHash,
            cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        var user = await _userOAuthRepository.GetUserByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User session is invalid.");

        if (!user.CanLogin())
        {
            throw new UnauthorizedAccessException("User is not allowed to login.");
        }

        var roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);

        if (roles.Count == 0)
        {
            await _userOAuthRepository.EnsureDefaultBuyerRoleAsync(user.Id, cancellationToken);

            // Persist the default role assignment before querying database-backed privileges.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);
        }

        // Refresh re-reads privileges so revoked rights disappear from new JWTs.
        var privileges = await _userOAuthRepository.GetActivePrivilegeCodesAsync(
            user.Id,
            cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            privileges);

        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);

        session.RotateRefreshToken(
            newRefreshTokenHash,
            _refreshTokenService.GetRefreshTokenExpiresAt());

        await _userOAuthRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: user.Id,
                targetUserId: user.Id,
                action: UserAuditActions.RefreshTokenRotated,
                entityType: nameof(UserSession),
                entityId: session.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Value,
            RefreshToken: newRefreshToken,
            ExpiresAt: accessToken.ExpiresAt,
            User: new AuthUserResponse(
                user.Id,
                user.Email,
                user.FullName,
                roles,
                privileges));
    }
}
