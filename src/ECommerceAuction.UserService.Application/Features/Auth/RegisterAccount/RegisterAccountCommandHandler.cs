using System.Security.Cryptography;
using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;
using Nexus.Shared.Contracts.Events.User;
namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

/// <summary>
/// Đạt - RegisterAccount: validates a local registration request, stores pending data in Redis,
/// and publishes an email OTP event instead of creating the SQL user immediately.
/// </summary>
public sealed class RegisterAccountCommandHandler
    : ICommandHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    private static readonly TimeSpan PendingRegistrationExpiration = TimeSpan.FromMinutes(10);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICacheService _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;
    public RegisterAccountCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICacheService cacheService,
        IPublishEndpoint publishEndpoint
        )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Receives register data, hashes the password, stores pending user data in cache,
    /// and asks the email flow to send an OTP.
    /// </summary>
    public async Task<RegisterAccountResponse> Handle(
        RegisterAccountCommand request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new InvalidOperationException("Phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (await _userRepository.CheckEmailExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("The email already exists.");
        }

        if (await _userRepository.CheckPhoneExistsAsync(phoneNumber, cancellationToken))
        {
            throw new InvalidOperationException("The phone number already exists.");
        }

        var pendingByEmailKey = BuildPendingEmailKey(email);
        var pendingByPhoneKey = BuildPendingPhoneKey(phoneNumber);

        var existingPendingByEmail = await _cacheService.GetAsync<PendingUserCacheModel>(
            pendingByEmailKey,
            cancellationToken);

        if (existingPendingByEmail is not null)
        {
            throw new InvalidOperationException("This email is already waiting for verification.");
        }

        var existingPendingByPhone = await _cacheService.GetAsync<PendingUserCacheModel>(
            pendingByPhoneKey,
            cancellationToken);

        if (existingPendingByPhone is not null)
        {
            throw new InvalidOperationException("This phone number is already waiting for verification.");
        }

        var pendingUser = new PendingUserCacheModel(
            Id: Guid.NewGuid(),
            Email: email,
            PhoneNumber: phoneNumber,
            FullName: fullName,
            PasswordHash: _passwordHasher.HashPassword(request.Password),
            OtpCode: GenerateOtpCode(),
            ExpiresAt: DateTime.UtcNow.Add(PendingRegistrationExpiration));

        // Đạt - RegisterAccount: keep pending data outside SQL until VerifyEmail succeeds.
        await _cacheService.SetAsync(
            pendingByEmailKey,
            pendingUser,
            PendingRegistrationExpiration,
            cancellationToken);

        await _cacheService.SetAsync(
            pendingByPhoneKey,
            pendingUser,
            PendingRegistrationExpiration,
            cancellationToken);

        // Đạt + Kiệt: Kiệt's Email Service will consume this event and send the OTP to the user.
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = pendingUser.Id,
            Email = pendingUser.Email,
            FullName =  pendingUser.FullName,
            OtpCode = pendingUser.OtpCode,
            OtpExpiresAt = pendingUser.ExpiresAt,
            SourceService = "UserService", 
            CorrelationId = Guid.NewGuid() 
        }, cancellationToken);

        return new RegisterAccountResponse(
            pendingUser.Id,
            pendingUser.Email,
            pendingUser.PhoneNumber,
            pendingUser.FullName,
            "PENDING_EMAIL_VERIFICATION");
    }

    internal static string BuildPendingEmailKey(string email)
    {
        return $"auth:register:pending:email:{NormalizeEmail(email)}";
    }

    internal static string BuildPendingPhoneKey(string phoneNumber)
    {
        return $"auth:register:pending:phone:{NormalizePhoneNumber(phoneNumber)}";
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return phoneNumber.Trim();
    }

    private static string GenerateOtpCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }
}
