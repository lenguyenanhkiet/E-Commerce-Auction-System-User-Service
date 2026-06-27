using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Roles.Common;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Roles.UpdateRole;

/// <summary>
/// Role Management: updates a custom role and synchronizes its current privilege assignments.
/// </summary>
public sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, RoleResponse>
{
    private readonly IRoleManagementRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRoleManagementRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleResponse> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");

        var role = await _repository.GetRoleByIdWithPrivilegesAsync(request.RoleId, cancellationToken);

        if (role is null || role.Status == RoleStatuses.Deleted)
        {
            throw new NotFoundException("Role was not found.");
        }

        if (role.IsSystemRole)
        {
            throw new BusinessRuleException("System roles cannot be updated by API.");
        }

        // Capture the old state before mutating the aggregate for audit comparison.
        var oldPrivilegeCodes = role.RolePrivileges
            .Select(rolePrivilege => rolePrivilege.Privilege.Code)
            .ToArray();
        var oldAuditJson = RoleAuditSerializer.Serialize(role, oldPrivilegeCodes);

        // The update request is treated as the desired final privilege set.
        var normalizedPrivilegeCodes = request.PrivilegeCodes
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var requestedPrivileges = await _repository.GetPrivilegesByCodesAsync(
            normalizedPrivilegeCodes,
            cancellationToken);

        if (requestedPrivileges.Count != normalizedPrivilegeCodes.Length)
        {
            throw new BusinessRuleException("One or more privilege codes are invalid or inactive.");
        }

        role.UpdateInfo(request.Name, request.Description);

        var requestedPrivilegeIds = requestedPrivileges
            .Select(privilege => privilege.Id)
            .ToHashSet();
        var existingPrivilegeIds = role.RolePrivileges
            .Select(rolePrivilege => rolePrivilege.PrivilegeId)
            .ToHashSet();

        // Remove assignments that are no longer present in the requested final set.
        foreach (var assignment in role.RolePrivileges
                     .Where(assignment => !requestedPrivilegeIds.Contains(assignment.PrivilegeId))
                     .ToArray())
        {
            _repository.RemoveRolePrivilege(assignment);
        }

        // Add only new assignments and keep existing assignments untouched.
        foreach (var privilege in requestedPrivileges.Where(
                     privilege => !existingPrivilegeIds.Contains(privilege.Id)))
        {
            await _repository.AddRolePrivilegeAsync(
                RolePrivilege.Assign(role.Id, privilege.Id, actorUserId),
                cancellationToken);
        }

        var newAuditJson = RoleAuditSerializer.Serialize(role, normalizedPrivilegeCodes);

        await _repository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: null,
                UserAuditActions.RoleUpdated,
                nameof(Role),
                role.Id.ToString(),
                oldAuditJson,
                newAuditJson),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RoleMapper.ToResponse(role, requestedPrivileges);
    }
}
