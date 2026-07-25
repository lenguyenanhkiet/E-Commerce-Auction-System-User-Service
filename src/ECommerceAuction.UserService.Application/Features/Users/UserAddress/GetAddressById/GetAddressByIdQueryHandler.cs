using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetAddressById;

public sealed class GetAddressByIdQueryHandler : IQueryHandler<GetAddressByIdQuery, GetAddressByIdQueryResponse>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAddressByIdQueryHandler(IAddressRepository addressRepository, ICurrentUserService currentUserService)
    {
        _addressRepository = addressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAddressByIdQueryResponse> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information not found in JWT.");
        var address = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken) ?? throw new NotFoundException($"Address {request.AddressId} not found.");

        // Ensure the address belongs to the logged-in user (to prevent IDOR).
        if (address.UserId != userId)
        {
            // When an address exists but belongs to someone else, intentionally throw a NotFoundException (404) instead of a 403 — so as not to reveal that "this address exists".
            throw new NotFoundException($"Address {request.AddressId} not found.");
        }

        return new GetAddressByIdQueryResponse
            (
            address.Id,
            address.RecipientName,
            address.RecipientPhone,
            address.Street,
            address.Province,
            address.Ward,
            address.Type,
            address.IsDefault,
            address.CreatedAt,
            address.UpdatedAt
            );
    }
}