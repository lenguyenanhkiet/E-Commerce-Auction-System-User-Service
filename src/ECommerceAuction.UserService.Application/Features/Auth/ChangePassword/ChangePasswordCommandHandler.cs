using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandHandler"/> class.
    /// </summary>
    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Changes the password of the authenticated user after validating the current password.
    /// </summary>
    public async Task<ChangePasswordResponse> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new Exception("User not found.");
        }

        var oldPasswordMatched = _passwordHasher.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash);

        if (!oldPasswordMatched)
        {
            throw new Exception("Current password is incorrect.");
        }

        var newPasswordSameAsOld = _passwordHasher.VerifyPassword(
            request.NewPassword,
            user.PasswordHash);

        if (newPasswordSameAsOld)
        {
            throw new Exception("New password must be different from current password.");
        }

        var newPasswordHash = _passwordHasher.HashPassword(
            request.NewPassword);

        user.ChangePassword(newPasswordHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} changed password successfully.",
            user.Id);

        return new ChangePasswordResponse(
            user.Id,
            user.PasswordChangedAt!.Value);
    }
}