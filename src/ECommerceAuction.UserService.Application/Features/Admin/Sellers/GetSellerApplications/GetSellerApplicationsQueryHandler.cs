using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.GetSellerApplications
{
    public sealed class GetSellerApplicationsQueryHandler : IQueryHandler<GetSellerApplicationsQuery, PagedSellerApplicationsResponse>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public GetSellerApplicationsQueryHandler(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<PagedSellerApplicationsResponse> Handle(GetSellerApplicationsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _sellerProfileRepository.GetPagedAsync(
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

            var summaries = items
                .Select(p => new SellerApplicationSummaryResponse(p.Id, p.UserId, p.SellerType, p.Status, p.BusinessName, p.Address, p.BankName, p.BankAccountNumber, p.BankAccountHolder, p.SubmittedAt))
                .ToList();

            return new PagedSellerApplicationsResponse(summaries, totalCount, request.Page, request.PageSize);
        }
    }
}