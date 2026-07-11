using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.DeleteAddress;

public sealed record DeleteAddressCommand(Guid AddressId) : ICommand;