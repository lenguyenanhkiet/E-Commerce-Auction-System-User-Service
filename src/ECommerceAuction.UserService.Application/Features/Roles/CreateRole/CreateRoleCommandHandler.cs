using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Roles.Common;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Entities.Roles;

namespace ECommerceAuction.UserService.Application.Features.Roles.CreateRole;

/// <summary>
/// Role Management: creates a custom role, assigns privileges, and records one audit snapshot.
/// </summary>
public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, RoleResponse>
{
    private readonly IRoleManagementRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleManagementRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleResponse> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetRequiredActorUserId();
        var normalizedRoleCode = request.Code.Trim().ToUpperInvariant();

        if (await _repository.RoleCodeExistsAsync(normalizedRoleCode, null, cancellationToken))
        {
            throw new ConflictException("Role code already exists.");
        }

        // Normalize and de-duplicate privilege codes so the database receives one assignment per privilege.
        var normalizedPrivilegeCodes = NormalizePrivilegeCodes(request.PrivilegeCodes);
        var privileges = await _repository.GetPrivilegesByCodesAsync(
            normalizedPrivilegeCodes,
            cancellationToken);

        if (privileges.Count != normalizedPrivilegeCodes.Length)
        {
            throw new BusinessRuleException("One or more privilege codes are invalid or inactive.");
        }

        var role = Role.CreateCustom(normalizedRoleCode, request.Name, request.Description);
        await _repository.AddRoleAsync(role, cancellationToken);

        foreach (var privilege in privileges)
        {
            await _repository.AddRolePrivilegeAsync(
                RolePrivilege.Assign(role.Id, privilege.Id, actorUserId),
            cancellationToken);
        }

        // Store the role and privilege state before saving so audit history can explain what changed.
        var newAuditJson = RoleAuditSerializer.Serialize(role, normalizedPrivilegeCodes);

        await _repository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: null,
                UserAuditActions.RoleCreated,
                nameof(Role),
                role.Id.ToString(),
                oldValue: null,
                newValue: newAuditJson),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RoleMapper.ToResponse(role, privileges);
    }

    private Guid GetRequiredActorUserId()
    {
        return _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
    }

    private static string[] NormalizePrivilegeCodes(IEnumerable<string> privilegeCodes)
    {
        return privilegeCodes
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
