using System.Text.Json;
using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.User;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.UpdateUser;

/// <summary>
/// Admin User Management: updates a user's editable fields, optionally reconciles roles,
/// and records before/after audit snapshots.
/// </summary>
public sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleManagementRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IRoleManagementRepository roleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UpdateUserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found or has already been deleted.");

        var oldSnapshot = Snapshot(user);

        // Apply the email change first so a conflict aborts before any other field is touched.
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var newEmail = request.Email.Trim().ToLowerInvariant();

            if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _userRepository.CheckEmailExistsForOtherUserAsync(
                    newEmail,
                    user.Id,
                    cancellationToken);

                if (emailTaken)
                {
                    throw new ConflictException("Email is already in use by another account.");
                }

                user.ChangeEmailByAdmin(newEmail);
            }
        }

        user.AdminUpdate(
            fullName: request.FullName,
            gender: request.Gender,
            dateOfBirth: request.DateOfBirth
           );

        await _userRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: user.Id,
                UserAuditActions.UserUpdated,
                nameof(User),
                user.Id.ToString(),
                oldValue: oldSnapshot,
                newValue: Snapshot(user)),
            cancellationToken);

        // A null RoleCodes means "leave roles unchanged"; an explicit list reconciles them.
        IReadOnlyList<string> finalRoleCodes;
        var rolesChanged = false;
        if (request.RoleCodes is null)
        {
            finalRoleCodes = await GetActiveRoleCodesAsync(user, cancellationToken);
        }
        else
        {
            (finalRoleCodes, rolesChanged) =
                await ReconcileRolesAsync(user, request.RoleCodes, actorUserId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration events only after the change is durably persisted.
        await _publishEndpoint.Publish(
            new UserUpdatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SourceService = "UserService"
            },
            cancellationToken);

        if (rolesChanged)
        {
            await _publishEndpoint.Publish(
                new UserRoleAssignedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = finalRoleCodes,
                    SourceService = "UserService"
                },
                cancellationToken);
        }

        return new UpdateUserResponse(user.Id, IsUpdated: true, Roles: finalRoleCodes);
    }

    /// <summary>
    /// Reconciles the user's active roles to exactly match the requested codes, revoking and
    /// assigning as needed, and records a role-change audit entry when anything changes.
    /// </summary>
    private async Task<(IReadOnlyList<string> Codes, bool Changed)> ReconcileRolesAsync(
        User user,
        IReadOnlyList<string> requestedRoleCodes,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var normalizedCodes = requestedRoleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var desiredRoles = await ResolveActiveRolesAsync(normalizedCodes, cancellationToken);
        var desiredRoleIds = desiredRoles.Select(role => role.Id).ToHashSet();
        var codeByRoleId = desiredRoles.ToDictionary(role => role.Id, role => role.Code);

        // Capture the current active roles (code + id) before mutating for the audit trail.
        var currentActiveAssignments = user.UserRoles
            .Where(userRole => userRole.RevokedAt is null && userRole.Status == UserRoleStatuses.Active)
            .ToList();
        var currentRoleIds = currentActiveAssignments.Select(userRole => userRole.RoleId).ToHashSet();
        var oldRoleCodes = await ResolveRoleCodesByIdsAsync(currentRoleIds, cancellationToken);

        // Revoke assignments the admin removed.
        foreach (var assignment in currentActiveAssignments.Where(a => !desiredRoleIds.Contains(a.RoleId)))
        {
            assignment.Revoke();
        }

        // Assign roles the user does not already hold.
        foreach (var roleId in desiredRoleIds.Where(id => !currentRoleIds.Contains(id)))
        {
            user.AssignRole(UserRole.Assign(user.Id, roleId, actorUserId));
        }

        var newRoleCodes = desiredRoles.Select(role => role.Code).OrderBy(code => code).ToList();

        var rolesChanged = !desiredRoleIds.SetEquals(currentRoleIds);
        if (rolesChanged)
        {
            await _userRepository.AddAuditLogAsync(
                UserAuditLog.Create(
                    actorUserId,
                    targetUserId: user.Id,
                    UserAuditActions.UserRoleAssigned,
                    nameof(User),
                    user.Id.ToString(),
                    oldValue: JsonSerializer.Serialize(new { Roles = oldRoleCodes.OrderBy(code => code) }),
                    newValue: JsonSerializer.Serialize(new { Roles = newRoleCodes })),
                cancellationToken);
        }

        return (newRoleCodes, rolesChanged);
    }

    /// <summary>
    /// Resolves and validates that every requested role code maps to an active, non-deleted role.
    /// </summary>
    private async Task<IReadOnlyList<Role>> ResolveActiveRolesAsync(
        IReadOnlyCollection<string> normalizedCodes,
        CancellationToken cancellationToken)
    {
        if (normalizedCodes.Count == 0)
        {
            return [];
        }

        var query = _roleRepository.QueryRoles()
            .Where(role =>
                role.Status == RoleStatuses.Active &&
                role.DeletedAt == null &&
                normalizedCodes.Contains(role.Code));

        var roles = await _roleRepository.ListRolesAsync(query, cancellationToken);

        if (roles.Count != normalizedCodes.Count)
        {
            var foundCodes = roles.Select(role => role.Code).ToHashSet(StringComparer.Ordinal);
            var invalidCodes = normalizedCodes.Where(code => !foundCodes.Contains(code));

            throw new BusinessRuleException(
                $"One or more roles are invalid or inactive: {string.Join(", ", invalidCodes)}.");
        }

        return [.. roles];
    }

    private async Task<IReadOnlyList<string>> GetActiveRoleCodesAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var activeRoleIds = user.UserRoles
            .Where(userRole => userRole.RevokedAt is null && userRole.Status == UserRoleStatuses.Active)
            .Select(userRole => userRole.RoleId)
            .ToHashSet();

        var codes = await ResolveRoleCodesByIdsAsync(activeRoleIds, cancellationToken);
        return codes.OrderBy(code => code).ToList();
    }

    private async Task<IReadOnlyList<string>> ResolveRoleCodesByIdsAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var query = _roleRepository.QueryRoles().Where(role => roleIds.Contains(role.Id));
        var roles = await _roleRepository.ListRolesAsync(query, cancellationToken);
        return roles.Select(role => role.Code).ToList();
    }

    private static string Snapshot(User user)
    {
        return JsonSerializer.Serialize(new
        {
            user.FullName,
            user.Email,
            user.Gender,
            user.DateOfBirth,
        });
    }
}