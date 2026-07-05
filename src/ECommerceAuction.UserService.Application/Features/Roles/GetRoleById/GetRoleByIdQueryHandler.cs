using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Roles.Common;
using ECommerceAuction.UserService.Domain.Entities.Roles;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetRoleById;

public sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleResponse>
{
    private readonly IRoleManagementRepository _repository;

    public GetRoleByIdQueryHandler(IRoleManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoleResponse> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleByIdWithPrivilegesAsync(
            request.RoleId,
            cancellationToken);

        if (role is null || role.Status == RoleStatuses.Deleted)
        {
            throw new NotFoundException("Role was not found.");
        }

        return RoleMapper.ToResponse(role);
    }
}
