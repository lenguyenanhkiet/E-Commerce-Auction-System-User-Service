using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers;

/// <summary>
/// Type of seller account
/// </summary>
public static class SellerTypes
{
    public const string Individual = "Individual";
    public const string Business = "Business";

    public static bool IsValid(string? value)
    {
        return value is Individual or Business;
    }
}