using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;

/// <summary>
/// Email verification flow: converts a Redis pending registration into a real SQL user after OTP verification.
/// </summary>
public sealed class VerifyEmailCommandHandler
    : ICommandHandler<VerifyEmailCommand, Guid>
{
    private readonly ICacheService _cacheService;
    private readonly IUserRepository _userRepository;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        ICacheService cacheService,
        IUserRepository userRepository,
        IUserOAuthRepository userOAuthRepository,
        IUnitOfWork unitOfWork)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
        _userOAuthRepository = userOAuthRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Validates the OTP, creates the user, creates default reputation, assigns BUYER, and clears pending cache.
    /// </summary>
    public async Task<Guid> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var pendingKey = RegisterAccountCommandHandler.BuildPendingEmailKey(email);

        var pendingUser = await _cacheService.GetAsync<PendingUserCacheModel>(
            pendingKey,
            cancellationToken)
            ?? throw new InvalidOperationException("Registration request is expired or not found.");

        if (pendingUser.ExpiresAt <= DateTime.UtcNow)
        {
            await RemovePendingRegistrationAsync(pendingUser, cancellationToken);
            throw new InvalidOperationException("OTP has expired.");
        }

        if (!string.Equals(pendingUser.OtpCode, request.OtpCode.Trim(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("OTP is invalid.");
        }

        if (await _userRepository.CheckEmailExistsAsync(pendingUser.Email, cancellationToken))
        {
            await RemovePendingRegistrationAsync(pendingUser, cancellationToken);
            throw new InvalidOperationException("The email already exists.");
        }

        if (await _userRepository.CheckPhoneExistsAsync(pendingUser.PhoneNumber, cancellationToken))
        {
            await RemovePendingRegistrationAsync(pendingUser, cancellationToken);
            throw new InvalidOperationException("The phone number already exists.");
        }
        // Create new account
        var user = new User(
            pendingUser.Id,
            pendingUser.Email,
            pendingUser.PasswordHash,
            pendingUser.FullName,
            pendingUser.PhoneNumber);

        var now = DateTime.UtcNow;
        // A successfully verified email gives the user the first reputation point.
        var reputationProfile = new ReputationProfile
        {
            UserId = user.Id,
            Score = 1,
            TrustLevel = "Silver",
            UpdatedAt = now
        };

        // SQL user is created only after the email OTP is correct.
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.AddReputationProfileAsync(reputationProfile, cancellationToken);

        // Every verified local account receives the default BUYER role for future token claims.
        await _userOAuthRepository.EnsureDefaultBuyerRoleAsync(user.Id, cancellationToken);

        await _userOAuthRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: user.Id,
                targetUserId: user.Id,
                action: UserAuditActions.LocalRegisterVerified,
                entityType: nameof(User),
                entityId: user.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RemovePendingRegistrationAsync(pendingUser, cancellationToken);

        return user.Id;
    }

    private async Task RemovePendingRegistrationAsync(
        PendingUserCacheModel pendingUser,
        CancellationToken cancellationToken)
    {
        await _cacheService.RemoveAsync(
            RegisterAccountCommandHandler.BuildPendingEmailKey(pendingUser.Email),
            cancellationToken);

        await _cacheService.RemoveAsync(
            RegisterAccountCommandHandler.BuildPendingPhoneKey(pendingUser.PhoneNumber),
            cancellationToken);
    }
}
