using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.PaymentMethods;

public static class BankAccountVerificationStatuses
{
    public const string Pending = "Pending";
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
    public const string Revoked = "Revoked";
    public const string Expired = "Expired";

    public static bool IsValid(string? value)
    {
        return value is Pending or Verified or Rejected or Revoked or Expired;
    }
}