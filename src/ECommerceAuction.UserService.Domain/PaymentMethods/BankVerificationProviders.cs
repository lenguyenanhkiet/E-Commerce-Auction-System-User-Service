using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.PaymentMethods;

public static class BankVerificationProviders
{
    public const string Sandbox = "SANDBOX";
    public const string BankPartner = "BANK_PARTNER";

    public static bool IsValid(string? provider)
    {
        return provider is Sandbox or BankPartner;
    }
}