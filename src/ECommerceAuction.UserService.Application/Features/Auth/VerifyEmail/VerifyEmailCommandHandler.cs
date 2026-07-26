using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Reputation;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.User;

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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IdentityVerifications.VerifyEmail.CompleteEmailVerificationService _completeEmailVerification;
    public VerifyEmailCommandHandler(
        ICacheService cacheService,
        IUserRepository userRepository,
        IUserOAuthRepository userOAuthRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IdentityVerifications.VerifyEmail.CompleteEmailVerificationService completeEmailVerification)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
        _userOAuthRepository = userOAuthRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _completeEmailVerification = completeEmailVerification;
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

        if (pendingUser.ExpiresAt <= DateTimeOffset.UtcNow)
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

        var now = DateTimeOffset.UtcNow;

        // SQL user is created only after the email OTP is correct.
        await _userRepository.AddAsync(user, cancellationToken);
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

        // Creates the buyer verification profile, marks email verified, writes the confirmed
        // EMAIL_VERIFIED ledger entry (+1) and reputation summary, then commits everything above
        // (user, role, audit) in a single transaction via its SaveChanges.
        await _completeEmailVerification.CompleteAsync(user.Id, now, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            SourceService = "UserService",
            CorrelationId = pendingUser.CorrelationId
        }, cancellationToken);
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
