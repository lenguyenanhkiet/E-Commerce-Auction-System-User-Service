using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.Common;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.ExchangeLoginCode;

/// <summary>
/// Handles the final OAuth2 step where the frontend exchanges a one-time login code for system tokens.
/// </summary>
public sealed class ExchangeLoginCodeCommandHandler
    : ICommandHandler<ExchangeLoginCodeCommand, AuthResponse>
{
    private readonly ILoginCodeService _loginCodeService;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public ExchangeLoginCodeCommandHandler(
        ILoginCodeService loginCodeService,
        IUserOAuthRepository userOAuthRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork)
    {
        _loginCodeService = loginCodeService;
        _userOAuthRepository = userOAuthRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Validates the one-time login code, issues JWT/refresh tokens, creates a session, and returns user auth data.
    /// </summary>
    public async Task<AuthResponse> Handle(
        ExchangeLoginCodeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await _loginCodeService.ValidateAndConsumeAsync(
            request.Code,
            cancellationToken);

        var user = await _userOAuthRepository.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User was not found.");

        if (!user.CanLogin())
        {
            throw new InvalidOperationException("User is not allowed to login.");
        }

        var roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);

        if (roles.Count == 0)
        {
            // ECA-6 OAuth2 Google: keep legacy/test users usable by assigning the default BUYER role if no role exists.
            await _userOAuthRepository.EnsureDefaultBuyerRoleAsync(user.Id, cancellationToken);

            // Persist the default role assignment before querying database-backed privileges.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);
        }

        // Google login uses the same database-backed RBAC claims as local login.
        var privileges = await _userOAuthRepository.GetActivePrivilegeCodesAsync(
            user.Id,
            cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            privileges);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);
        var refreshTokenExpiresAt = _refreshTokenService.GetRefreshTokenExpiresAt();

        var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt);

        await _userOAuthRepository.AddUserSessionAsync(session, cancellationToken);

        await _userOAuthRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: user.Id,
                targetUserId: user.Id,
                action: UserAuditActions.RefreshTokenIssued,
                entityType: nameof(UserSession),
                entityId: session.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Value,
            RefreshToken: refreshToken,
            ExpiresAt: accessToken.ExpiresAt,
            Status: user.Status
            );
    }
}