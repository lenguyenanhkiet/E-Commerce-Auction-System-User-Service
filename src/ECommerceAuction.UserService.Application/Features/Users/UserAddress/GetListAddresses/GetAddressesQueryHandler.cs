using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetListAddresses;

public sealed class GetAddressesQueryHandler : IQueryHandler<GetAddressesQuery, IReadOnlyList<GetAddressesResponse>>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAddressesQueryHandler(IAddressRepository addressRepository, ICurrentUserService currentUserService)
    {
        _addressRepository = addressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<GetAddressesResponse>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information not found in JWT.");
        var addresses = await _addressRepository.GetUserAddressesAsync(user, cancellationToken);

        return addresses.Select(a => new GetAddressesResponse
        (
            a.Id,
            a.RecipientName,
            a.RecipientPhone,
            a.Street,
            a.Province,
            a.Ward,
            a.Type,
            a.IsDefault,
            a.CreatedAt
        )).ToList();
    }
}