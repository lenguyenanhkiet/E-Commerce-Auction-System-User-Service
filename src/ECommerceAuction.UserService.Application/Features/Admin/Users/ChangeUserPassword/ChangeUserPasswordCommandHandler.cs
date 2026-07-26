using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.User;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.ChangeUserPassword;

/// <summary>
/// Admin User Management: sets another user's password and records the action.
/// </summary>
public sealed class ChangeUserPasswordCommandHandler
    : ICommandHandler<ChangeUserPasswordCommand, ChangeUserPasswordResponse>
{
    private const int KeepHistoryEntries = 5;

    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public ChangeUserPasswordCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHistoryRepository passwordHistoryRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ChangeUserPasswordResponse> Handle(
        ChangeUserPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found or has already been deleted.");

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        var changedAt = DateTimeOffset.UtcNow;

        user.ChangePassword(newPasswordHash);

        // Force the user to set their own password on next login unless the admin opts out.
        if (request.RequireChangeOnNextLogin)
        {
            user.FlagMustChangePassword();
        }

        await _passwordHistoryRepository.AddAsync(
            UserPasswordHistory.Create(user.Id, newPasswordHash),
            cancellationToken);
        await _passwordHistoryRepository.PruneOldEntriesAsync(
            user.Id,
            KeepHistoryEntries,
            cancellationToken);

        await _userRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: user.Id,
                UserAuditActions.UserPasswordChangedByAdmin,
                nameof(User),
                user.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserPasswordChangedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                ChangedByAdmin = true,
                ChangedAt = changedAt.UtcDateTime,
                SourceService = "UserService"
            },
            cancellationToken);

        return new ChangeUserPasswordResponse(user.Id, changedAt);
    }
}
