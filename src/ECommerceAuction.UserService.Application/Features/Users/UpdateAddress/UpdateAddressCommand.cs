using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateAddress;
/// <summary>
/// Command to update an existing user address.
/// </summary>
public sealed record UpdateAddressCommand(
    Guid AddressId,
    [StringLength(200)] string RecipientName,
    [StringLength(12)] string RecipientPhone,
    [StringLength(50)] string Province,
    [StringLength(50)] string City,
    [StringLength(50)] string Ward,
    [StringLength(255)] string Street,
    [StringLength(50)] string Type,
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
    string Province,
    string City,
    string Ward,
    string Type,
    bool IsDefault,
    DateTime? UpdatedAt);
