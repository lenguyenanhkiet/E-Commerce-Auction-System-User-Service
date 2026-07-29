using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.GetSellerApplications
{
    public sealed record GetSellerApplicationsQuery(
        string? Status,
        int Page = 1,
        int PageSize = 20) : IQuery<PagedSellerApplicationsResponse>;

    public sealed record SellerApplicationSummaryResponse(
    Guid Id,
    Guid UserId,
    string SellerType,
    string Status,
    string BusninessName,
    string Address,
    string TaxCode,
    string BankAccountNumber,
    string BankName,
    string BankAccountHolder,
    DateTimeOffset SubmittedAt);

    // ASSUMPTION: your project likely already has a shared PagedResult<T> type used by
    // GetUsersQuery — if so, delete this and reuse that one instead of duplicating paging shape.
    public sealed record PagedSellerApplicationsResponse(
        IReadOnlyList<SellerApplicationSummaryResponse> Items,
        int TotalCount,
        int Page,
        int PageSize);
}