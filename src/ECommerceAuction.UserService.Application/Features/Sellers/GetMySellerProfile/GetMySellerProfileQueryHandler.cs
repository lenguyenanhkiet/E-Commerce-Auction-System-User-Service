using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Domain.Sellers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.GetMySellerProfile
{
    public sealed class GetMySellerProfileQueryHandler : IQueryHandler<GetMySellerProfileQuery, SellerProfileResponse?>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public GetMySellerProfileQueryHandler(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<SellerProfileResponse?> Handle(GetMySellerProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await _sellerProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            return profile is null
              ? null
              : new SellerProfileResponse(
                  profile.Id,
                  profile.SellerType,
                  profile.Status,
                  profile.RejectReason,
                  profile.SubmittedAt,
                  profile.ReviewedAt);
        }
    }
}