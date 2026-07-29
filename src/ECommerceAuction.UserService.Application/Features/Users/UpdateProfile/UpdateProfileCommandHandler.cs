using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using System.Security.Cryptography;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateProfile;

/// <summary>
/// Updates profile information and publishes an email verification event
/// when the user requests a new email.
/// </summary>
public sealed class UpdateProfileCommandHandler
    : ICommandHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IPublishEndpoint publishEndpoint)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UpdateProfileResponse> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
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

        // Phone number and address can be updated immediately.
        user.UpdateProfile(
            request.PhoneNumber
            );

        var emailVerificationSent = false;

        if (!string.IsNullOrWhiteSpace(request.NewEmail))
        {
            var newEmail = request.NewEmail
                .Trim()
                .ToLowerInvariant();

            // Do nothing when the submitted email is the current email.
            if (!string.Equals(
                    newEmail,
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var emailExists =
                    await _userRepository.CheckEmailExistsForOtherUserAsync(
                        newEmail,
                        userId,
                        cancellationToken);

                if (emailExists)
                {
                    throw new InvalidOperationException(
                        "Email is already in use by another account.");
                }

                // Generate a random one-time verification token.
                var verificationToken = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32));

                var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(15);

                var pendingEmailChange = new PendingEmailChange(
                    UserId: userId,
                    NewEmail: newEmail,
                    ExpiresAtUtc: expiresAtUtc);

                // Store pending email change in Redis for 15 minutes.
                await _cacheService.SetAsync(
                    $"profile:email-change:{verificationToken}",
                    pendingEmailChange,
                    TimeSpan.FromMinutes(15),
                    cancellationToken);

                // User Service only publishes the event.
                // Notification Service is responsible for sending the email.
                var emailEvent = new ProfileEmailChangeRequested(
                    UserId: user.Id,
                    CurrentEmail: user.Email,
                    NewEmail: newEmail,
                    FullName: user.FullName,
                    VerificationToken: verificationToken,
                    ExpiresAtUtc: expiresAtUtc,
                    CorrelationId: Guid.NewGuid());

                await _publishEndpoint.Publish(
                    emailEvent,
                    cancellationToken);

                emailVerificationSent = true;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = emailVerificationSent
            ? "Profile updated successfully. A verification email has been sent to the new email address."
            : "Profile updated successfully.";

        return new UpdateProfileResponse(
            IsUpdated: true,
            EmailVerificationSent: emailVerificationSent,
            Message: message);
    }

    /// <summary>
    /// Data temporarily stored in Redis until the user confirms the new email.
    /// </summary>
    private sealed record PendingEmailChange(
        Guid UserId,
        string NewEmail,
        DateTimeOffset ExpiresAtUtc);
}