using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.ResubmitSeller
{
    public sealed record ResubmitSellerCommand 
        (
        Guid UserId,
        string? BusinessName,
        string? TaxCode,
        string? BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder
        ) : ICommand<ResubmitSellerResponse>;
    public sealed record ResubmitSellerResponse(Guid Id, string Status, DateTime SubmittedAt);
}
