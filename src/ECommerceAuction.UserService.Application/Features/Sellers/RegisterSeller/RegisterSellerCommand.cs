using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.RegisterSeller
{
    public sealed record RegisterSellerCommand(
        Guid UserId,
        string SellerType,
        string? BusinessName,
        string? TaxCode,
        string? BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder
        ) : ICommand<RegisterSellerResponse>;
    public sealed record RegisterSellerResponse(
        Guid Id,
        string Status,
        DateTimeOffset SubmittedAt
    );
}
