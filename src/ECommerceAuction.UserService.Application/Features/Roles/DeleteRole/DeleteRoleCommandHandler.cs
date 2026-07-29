using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Roles.Common;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Roles;

namespace ECommerceAuction.UserService.Application.Features.Roles.DeleteRole;

/// <summary>
/// Role Management: soft-deletes an unused custom role and records the previous state.
/// </summary>
public sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRoleManagementRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(
        IRoleManagementRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
        var role = await _repository.GetRoleByIdWithPrivilegesAsync(
            request.RoleId,
            cancellationToken);

        if (role is null || role.Status == RoleStatuses.Deleted)
        {
            throw new NotFoundException("Role was not found.");
        }

        if (role.IsSystemRole)
        {
            throw new BusinessRuleException("System roles cannot be deleted by API.");
        }

        // Do not delete roles that still grant permissions to active users.
        if (await _repository.RoleHasActiveUsersAsync(role.Id, cancellationToken))
        {
            throw new ConflictException(
                "Role cannot be deleted because it is assigned to active users.");
        }

        var oldAuditJson = RoleAuditSerializer.Serialize(
            role,
            role.RolePrivileges.Select(rolePrivilege => rolePrivilege.Privilege.Code));

        // Soft-delete keeps role history available for audit and avoids breaking old references.
        role.MarkDeleted();

        await _repository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: null,
                UserAuditActions.RoleDeleted,
                nameof(Role),
                role.Id.ToString(),
                oldAuditJson,
                newValue: null),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
