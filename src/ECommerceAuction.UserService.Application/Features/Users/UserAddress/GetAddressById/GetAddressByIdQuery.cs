using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetAddressById;

public sealed record GetAddressByIdQuery(Guid AddressId) : IQuery<GetAddressByIdQueryResponse>;

public sealed record GetAddressByIdQueryResponse
    (
        Guid Id,
        string RecipientName,
        string RecipientPhone,
        string Street,
        string Province,
        string Ward,
        string Type,
        bool IsDefault,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );