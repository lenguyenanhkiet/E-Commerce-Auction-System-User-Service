using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Sellers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.GetSellerApplicationById
{
    public sealed record GetSellerApplicationByIdQuery(Guid SellerProfileId) : IQuery<SellerApplicationDetailResponse>;

    public sealed record SellerApplicationDetailResponse(
        Guid Id,
        Guid UserId,
        string SellerType,
        string? BusinessName,
        string? TaxCode,
        string? BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder,
        string Status,
        string? RejectReason,
        DateTimeOffset SubmittedAt,
        DateTimeOffset? ReviewedAt,
        Guid? ReviewedBy);

    public sealed class GetSellerApplicationByIdQueryHandler
        : IQueryHandler<GetSellerApplicationByIdQuery, SellerApplicationDetailResponse>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public GetSellerApplicationByIdQueryHandler(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<SellerApplicationDetailResponse> Handle(
            GetSellerApplicationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var p = await _sellerProfileRepository.GetByIdAsync(request.SellerProfileId, cancellationToken)
                ?? throw new NotFoundException("Seller application not found.");

            return new SellerApplicationDetailResponse(
                p.Id, p.UserId, p.SellerType, p.BusinessName, p.TaxCode, p.BusinessLicenseUrl, p.Address,
                p.BankAccountNumber, p.BankName, p.BankAccountHolder, p.Status, p.RejectReason,
                p.SubmittedAt, p.ReviewedAt, p.ReviewedBy);
        }
    }
}
