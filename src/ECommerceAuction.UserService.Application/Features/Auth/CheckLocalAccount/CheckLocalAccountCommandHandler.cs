using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.Common;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

/// <summary>
///Dat + Duy - Local Login: validates email/phone credentials, issues JWT/refresh tokens,
/// creates a user session, and writes an audit log.
/// </summary>
public sealed class CheckLocalAccountCommandHandler
    : ICommandHandler<CheckLocalAccountCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckLocalAccountCommandHandler(
        IUserRepository userRepository,
        IUserOAuthRepository userOAuthRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userOAuthRepository = userOAuthRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Checks local account credentials and returns the same token response shape as Google OAuth2.
    /// </summary>
    public async Task<AuthResponse> Handle(
        CheckLocalAccountCommand request,
        CancellationToken cancellationToken)
    {
        var emailOrPhone = request.EmailOrPhone.Trim();

        if (string.IsNullOrWhiteSpace(emailOrPhone) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Incorrect login information.");
        }

        //Dat - Local Login: local accounts can log in with either email or phone number.
        var user = await _userRepository.GetByEmailOrPhoneAsync(
            emailOrPhone,
            cancellationToken)
            ?? throw new UnauthorizedAccessException("Incorrect login information.");

        if (user.IsBlockedFromAuthentication())
        {
            throw new UnauthorizedAccessException("User is not allowed to login.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("This account does not have a local password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Incorrect login information.");
        }

        user.MarkLoggedIn();

        var roles = await _userOAuthRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);

        if (roles.Count == 0)
        {
            // Duy - JWT/RBAC: make sure token claims use the database role source, not hard-coded roles.
            await _userOAuthRepository.EnsureDefaultBuyerRoleAsync(user.Id, cancellationToken);
            roles = [RoleCodes.Buyer];
        }

        // Duy - JWT: privilege claims are kept empty until RolePrivilege is merged into this source.
        IReadOnlyCollection<string> privileges = [];

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

        // Duy - JWT/Session: persist refresh token as a hashed session, never as plain text.
        await _userOAuthRepository.AddUserSessionAsync(session, cancellationToken);

        await _userOAuthRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: user.Id,
                targetUserId: user.Id,
                action: UserAuditActions.LocalLogin,
                entityType: nameof(User),
                entityId: user.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Value,
            RefreshToken: refreshToken,
            ExpiresAt: accessToken.ExpiresAt,
            User: new AuthUserResponse(
                user.Id,
                user.Email,
                user.FullName,
                roles));
    }
}
