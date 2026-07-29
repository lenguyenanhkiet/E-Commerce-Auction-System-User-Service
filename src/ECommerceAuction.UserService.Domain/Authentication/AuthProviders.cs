using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Authentication;

/// <summary>
/// Defines supported authentication providers for the system.
/// </summary>
public static class AuthProviders
{
    public const string Local = "LOCAL";
    public const string Google = "GOOGLE";
}