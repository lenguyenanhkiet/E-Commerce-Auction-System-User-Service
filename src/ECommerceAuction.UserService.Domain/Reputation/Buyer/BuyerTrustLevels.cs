using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;

/// <summary>
/// Supported buyer trust levels.
/// Values are stored as stable strings for database and API compatibility.
/// </summary>
public static class BuyerTrustLevels
{
    public const string Restricted = "RESTRICTED";
    public const string Basic = "BASIC";
    public const string Trusted = "TRUSTED";
    public const string Reliable = "RELIABLE";
    public const string Premium = "PREMIUM";
    public const string Elite = "ELITE";

    public static bool IsValid(string? value)
    {
        return value is
            Restricted or
            Basic or
            Trusted or
            Reliable or
            Premium or
            Elite;
    }
}