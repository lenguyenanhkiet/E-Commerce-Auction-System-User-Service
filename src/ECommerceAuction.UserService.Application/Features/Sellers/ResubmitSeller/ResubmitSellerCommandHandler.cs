using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Sellers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.ResubmitSeller
{
    public sealed class ResubmitSellerCommandHandler : ICommandHandler<ResubmitSellerCommand, ResubmitSellerResponse>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ResubmitSellerCommandHandler(ISellerProfileRepository sellerProfileRepository, IUnitOfWork unitOfWork)
        {
            _sellerProfileRepository = sellerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResubmitSellerResponse> Handle(ResubmitSellerCommand request, CancellationToken cancellationToken)
        {
            var profile = await _sellerProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken)
              ?? throw new NotFoundException("Seller application not found.");

            try
            {
                profile.Resubmit(
                    request.BusinessName,
                    request.TaxCode,
                    request.BusinessLicenseUrl,
                    request.Address,
                    request.BankAccountNumber,
                    request.BankName,
                    request.BankAccountHolder);
            }
            catch (InvalidOperationException ex)
            {
                // Domain guard (e.g. "only Rejected can be resubmitted") surfaced as a 400/409-style error.
                throw new BusinessRuleException(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ResubmitSellerResponse(profile.Id, profile.Status, profile.SubmittedAt);
        }
    }
}
