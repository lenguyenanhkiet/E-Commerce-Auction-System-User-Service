using System.Security.Cryptography;
using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;

namespace ECommerceAuction.UserService.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private static readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordCommandHandler"/> class.
    /// </summary>
    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Generates a password reset token, stores it, and publishes a password reset event.
    /// </summary>
    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            return new ForgotPasswordResponse(
                email,
                "If the email exists, a reset password link has been sent.");
        }

        var token = GenerateToken();

        var resetToken = PasswordResetToken.Create(
            user.Id,
            token,
            DateTime.UtcNow.Add(TokenExpiration));

        await _userRepository.AddPasswordResetTokenAsync(resetToken, cancellationToken);

        await _userRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId: null,
                targetUserId: user.Id,
                action: UserAuditLog.ForgotPasswordRequested,
                entityType: nameof(PasswordResetToken),
                entityId: resetToken.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
     
    //    await _publishEndpoint.Publish(
    //new PasswordResetRequestedEvent
    //{
    //    EventId = Guid.NewGuid(),
    //    UserId = user.Id,
    //    Email = user.Email,
    //    FullName = user.FullName,
    //    ResetToken = token,
    //    ExpiredAt = resetToken.ExpiryDate,
    //    RequestTime = DateTime.UtcNow
    //},
    //cancellationToken);

        return new ForgotPasswordResponse(
            email,
            "If the email exists, a reset password link has been sent.");
    }

    /// <summary>
    /// Generates a secure random token for password reset.
    /// </summary>
    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}