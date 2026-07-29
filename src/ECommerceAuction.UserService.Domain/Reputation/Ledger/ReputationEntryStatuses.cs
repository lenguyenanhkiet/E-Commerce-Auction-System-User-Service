using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

public static class ReputationEntryStatuses
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Reversed = "REVERSED";
    public const string Cancelled = "CANCELLED";

    public static bool IsValid(string? value)
    {
        return value is
            Pending or
            Confirmed or
            Reversed or
            Cancelled;
    }
}