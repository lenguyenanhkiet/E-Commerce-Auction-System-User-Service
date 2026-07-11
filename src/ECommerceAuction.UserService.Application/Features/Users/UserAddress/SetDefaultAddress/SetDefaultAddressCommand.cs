using System;
using System.Collections.Generic;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid AddressId) : ICommand;