using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.RegisterSeller
{
    public sealed record RegisterSellerCommand(
        string SellerType,
        string BusinessName,
        string ContactPhoneNumber,
        string TaxCode,
        string BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder
        ) : ICommand<RegisterSellerResponse>;
    public sealed record RegisterSellerResponse(
        Guid Id,
        string Status,
        string ContactPhoneNumber,
        DateTimeOffset SubmittedAt
    );
}