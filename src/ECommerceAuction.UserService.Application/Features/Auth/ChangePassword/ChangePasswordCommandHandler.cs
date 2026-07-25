using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions; // điều chỉnh namespace theo đúng project của bạn
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private const int CheckLastNPasswords = 3;
    private const int KeepHistoryEntries = 5;

    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHistoryRepository _passwordHistoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHistoryRepository passwordHistoryRepository,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChangePasswordResponse> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User {userId} not found.");

        var oldPasswordMatched = _passwordHasher.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash);

        if (!oldPasswordMatched)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Check trùng với password hiện tại (an toàn cho user cũ, trước khi có bảng history)
        var sameAsCurrent = _passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash);
        if (sameAsCurrent)
        {
            throw new BusinessRuleException("New password must be different from current password.");
        }

        // Check trùng với N mật khẩu gần nhất trong lịch sử
        var recentHashes = await _passwordHistoryRepository.GetRecentHashesAsync(
            user.Id, CheckLastNPasswords, cancellationToken);

        foreach (var oldHash in recentHashes)
        {
            if (_passwordHasher.VerifyPassword(request.NewPassword, oldHash))
            {
                throw new BusinessRuleException(
                    $"New password must not match any of the last {CheckLastNPasswords} passwords used.");
            }
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        var changedAt = DateTime.UtcNow;

        user.ChangePassword(newPasswordHash); // domain method: set PasswordHash, MustChangePassword = false, UpdatedAt

        await _passwordHistoryRepository.AddAsync(
            UserPasswordHistory.Create(user.Id, newPasswordHash), cancellationToken);
        await _passwordHistoryRepository.PruneOldEntriesAsync(
            user.Id, KeepHistoryEntries, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} changed password successfully.",
            user.Id);

        return new ChangePasswordResponse(user.Id, changedAt);
    }
}