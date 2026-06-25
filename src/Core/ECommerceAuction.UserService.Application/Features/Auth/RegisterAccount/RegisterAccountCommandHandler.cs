using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

public class RegisterAccountCommandHandler
    : ICommandHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;



    public RegisterAccountCommandHandler(
IUserRepository userRepository,
IPasswordHasher passwordHasher,
IUnitOfWork unitOfWork,
IEventBus eventBus)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;

    }

    /// <summary>
    /// Registers a new user account.
    /// Validates the email and phone number, checks for duplicate accounts,
    /// creates the user and reputation profile, saves the data,
    /// and publishes a user registration event.
    /// </summary>
    /// <param name="request">
    /// Contains the registration information such as email, phone number,
    /// full name, and password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Returns the newly created user's account information.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when:
    /// - Email is empty.
    /// - Phone number is empty.
    /// - Email already exists.
    /// - Phone number already exists.
    /// </exception>
    public async Task<RegisterAccountResponse> Handle(
        RegisterAccountCommand request,
        CancellationToken cancellationToken)
    {


        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new Exception("The email number is empty");
        }
        else if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new Exception("The phone number is empty");
        }
        else if (await _userRepository.CheckEmailExistsAsync(
          request.Email,
          cancellationToken))
        {
            throw new Exception("The email already exists.");
        }else if (await _userRepository.CheckPhoneExistsAsync(
              request.PhoneNumber,
              cancellationToken))
        {
            throw new Exception("The phone number already exists.");
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),

            Status = "PENDING",
            EmailVerified = false,
            PhoneVerified = false,
            FailedLoginAttempts = 0,

            CreatedAt = now,
            UpdatedAt = now
        };

        var profile = new ReputationProfile
        {
            UserId = user.Id,
            Score = 0,
            TrustLevel = "NORMAL",
            UpdatedAt = now
        };

        await _userRepository.AddAsync(user, cancellationToken);

        await _userRepository.AddReputationProfileAsync(profile, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new
        {
            EventName = "UserRegistered",
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName

        }, cancellationToken);

        return new RegisterAccountResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.FullName,
            user.Status);
    }
}