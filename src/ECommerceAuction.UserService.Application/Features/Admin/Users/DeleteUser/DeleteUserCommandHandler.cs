using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.Seller.V1;

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

        await _publishEndpoint.Publish(
            new SellerEligibilityChanged(
                NewId.NextGuid(),
                user.UpdatedAt ?? DateTimeOffset.UtcNow,
                user.Id,
                Found: true,
                UserStatus: user.Status,
                Deleted: true,
                SellerRoleActive: false,
                CanSell: false,
                EligibilityStatus: "INELIGIBLE",
                ReasonCode: "SELLER_DELETED",
                SourceVersion: (user.UpdatedAt ?? DateTimeOffset.UtcNow).UtcTicks),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
