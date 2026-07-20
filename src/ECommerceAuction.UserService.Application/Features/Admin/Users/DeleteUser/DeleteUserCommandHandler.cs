using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;
using Nexus.Contracts.Events.User;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.DeleteUser;

/// <summary>
/// Admin User Management: soft-deletes an existing user and records who performed the action.
/// </summary>
public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");

        // GetByIdAsync excludes soft-deleted users, so an already-deleted account resolves to null.
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found or has already been deleted.");

        if (user.Id == actorUserId)
        {
            throw new BusinessRuleException("You cannot delete your own account.");
        }

        user.AdminSoftDeleteUser();

        await _userRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: user.Id,
                UserAuditActions.UserDeleted,
                nameof(User),
                user.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserDeletedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DeletedAt = user.DeletedAt,
                SourceService = "UserService"
            },
            cancellationToken);
    }
}
