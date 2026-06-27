using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public class CheckLocalAccountCommandHandler
    : ICommandHandler<CheckLocalAccountCommand, CheckLocalAccountResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CheckLocalAccountCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Validates a local account by checking the email/phone number and password.
    /// Handles failed login attempts, locks the account after 5 consecutive failures,
    /// and updates the user's last login information on successful authentication.
    /// </summary>
    /// <param name="request">
    /// Contains the email or phone number and password used for authentication.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Returns the authenticated user's basic account information.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when:
    /// - The account does not exist.
    /// - The account is currently locked.
    /// - The password is incorrect.
    /// </exception>
    public async Task<CheckLocalAccountResponse> Handle(
        CheckLocalAccountCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailOrPhoneAsync(
            request.EmailOrPhone,
            cancellationToken);

        if (user is null)
        {
            throw new Exception("Incorrect login information");
        }
        else if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
        {
            throw new Exception("Your account has been locked. Please try again later!");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            user.FailedLoginAttempts += 1;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.FailedLoginAttempts >= 5)
            {
                user.Status = "LOCKED";
                user.LockedUntil = DateTime.UtcNow.AddHours(24);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new Exception("Incorrect login information");
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckLocalAccountResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.FullName,
            user.Status);
    }
}