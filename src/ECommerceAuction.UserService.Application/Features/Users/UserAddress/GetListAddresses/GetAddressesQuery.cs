using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetListAddresses;

public sealed record GetAddressesQuery() : IQuery<IReadOnlyList<GetAddressesResponse>>;

public sealed record GetAddressesResponse
    (
    Guid Id,
    string RecipientName,
    string RecipientPhone,
    string Street,
    string? Province,
    string? Ward,
    string? Type,
    bool IsDefault,
    DateTimeOffset CreatedAt
    );
