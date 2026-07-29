using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.UpdateAddress;
/// <summary>
/// Command to update an existing user address.
/// </summary>
public sealed record UpdateAddressCommand(
    Guid AddressId,
    string RecipientName,
    string RecipientPhone,
    string? Province,
    string? Ward,
    string Street,
    string? Type,
    bool IsDefault
    ) : ICommand<UpdateAddressResponse>;
/// <summary>
/// Response from update address command.
/// </summary>
public sealed record UpdateAddressResponse(
    Guid Id,
    string RecipientName,
    string RecipientPhone,
    string Street,
    string? Province,
    string? Ward,
    string? Type,
    bool IsDefault,
    DateTimeOffset? UpdatedAt);
