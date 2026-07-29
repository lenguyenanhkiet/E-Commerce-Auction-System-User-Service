using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.CreateAddress;

public sealed record CreateAddressCommand
    (
        string RecipientName,
        string RecipientPhone,
        string? Province,
        string? Ward,
        string Street,
        string? Type,
        bool IsDefault
    ) : ICommand<CreateAddressResponse>;
public sealed record CreateAddressResponse
    (
        Guid Id,
        string RecipientName,
        string RecipientPhone,
        string? Province,
        string? Ward,
        string Street,
        string? Type,
        bool IsDefault,
        DateTimeOffset CreatedAt
    );
