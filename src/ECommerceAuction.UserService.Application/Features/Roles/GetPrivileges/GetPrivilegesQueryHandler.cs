using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Roles.Common;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetPrivileges;

public sealed class GetPrivilegesQueryHandler
    : IQueryHandler<GetPrivilegesQuery, IReadOnlyCollection<PrivilegeResponse>>
{
    private readonly IRoleManagementRepository _repository;

    public GetPrivilegesQueryHandler(IRoleManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<PrivilegeResponse>> Handle(
        GetPrivilegesQuery request,
        CancellationToken cancellationToken)
    {
        var privileges = await _repository.GetActivePrivilegesAsync(cancellationToken);

        return privileges
            .Select(privilege => new PrivilegeResponse(
                privilege.Id,
                privilege.Code,
                privilege.Name,
                privilege.Description))
            .ToArray();
    }
}
