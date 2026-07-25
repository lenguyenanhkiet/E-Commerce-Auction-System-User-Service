using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordCommandHandler"/> class.
    /// </summary>
    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }


    /// <summary>
    /// Validates the password reset token and updates the user's password.
    /// </summary>
    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException("Token is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new InvalidOperationException("New password is required.");
        }

        var resetToken = await _userRepository.GetPasswordResetTokenAsync(
            request.Token,
            cancellationToken);

        if (resetToken is null || resetToken.IsUsed || resetToken.IsExpired())
        {
            throw new InvalidOperationException("Reset password token is invalid or expired.");
        }

        var user = await _userRepository.GetByIdAsync(
            resetToken.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var newPasswordSameAsOld = _passwordHasher.VerifyPassword(
            request.NewPassword,
            user.PasswordHash);

        if (newPasswordSameAsOld)
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        var changedAt = DateTime.UtcNow;

        user.ChangePassword(newPasswordHash); // domain method: set PasswordHash, MustChangePassword = false, UpdatedAt

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResponse(user.Id, changedAt);
    }
}