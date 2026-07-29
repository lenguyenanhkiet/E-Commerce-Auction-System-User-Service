using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.Seller.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.RejectSeller
{
    public sealed record RejectSellerCommand(Guid SellerProfileId, Guid AdminUserId, string Reason) : ICommand<RejectSellerResponse>;
    public sealed record RejectSellerResponse(Guid Id, string Status, string? RejectReason, DateTimeOffset ReviewAt);
    public sealed class RejectSellerCommandHandler
    : ICommandHandler<RejectSellerCommand, RejectSellerResponse>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;

        public RejectSellerCommandHandler(
            ISellerProfileRepository sellerProfileRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint)
        {
            _sellerProfileRepository = sellerProfileRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<RejectSellerResponse> Handle(
            RejectSellerCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 10)
            {
                throw new BusinessRuleException("A reject reason of at least 10 characters is required.");
            }

            var profile = await _sellerProfileRepository.GetByIdAsync(request.SellerProfileId, cancellationToken)
                ?? throw new NotFoundException("Seller application not found.");

            var user = await _userRepository.GetByIdAsync(profile.UserId, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var occurredAt = DateTimeOffset.UtcNow;
            try
            {
                profile.Reject(request.AdminUserId, request.Reason, occurredAt);
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessRuleException(ex.Message);
            }

            await _publishEndpoint.Publish(
                new SellerEligibilityChanged(
                    NewId.NextGuid(),
                    occurredAt,
                    user.Id,
                    Found: true,
                    UserStatus: user.Status,
                    Deleted: user.DeletedAt is not null,
                    SellerRoleActive: false,
                    CanSell: false,
                    EligibilityStatus: "INELIGIBLE",
                    ReasonCode: "SELLER_APPLICATION_REJECTED",
                    SourceVersion: occurredAt.UtcTicks),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RejectSellerResponse(profile.Id, profile.Status, profile.RejectReason!, profile.ReviewedAt!.Value);
        }
    }
}
