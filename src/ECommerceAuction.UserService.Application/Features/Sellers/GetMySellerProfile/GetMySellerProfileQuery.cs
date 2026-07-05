using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.GetMySellerProfile
{
    public sealed record GetMySellerProfileQuery(Guid UserId) : IQuery<SellerProfileResponse?>;
    public sealed record SellerProfileResponse(
        Guid Id,
        string SellerType,
        string Status,
        string? RejectReason,
        DateTime SubmittedAt,
        DateTime? ReviewedAt);
}
