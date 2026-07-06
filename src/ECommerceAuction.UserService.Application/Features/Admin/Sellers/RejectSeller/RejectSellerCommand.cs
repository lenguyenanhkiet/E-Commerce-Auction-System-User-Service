using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;
using Nexus.Shared.Contracts.Events.Seller;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.RejectSeller
{
    public sealed record RejectSellerCommand(Guid SellerProfileId, Guid AdminUserId, string Reason) : ICommand<RejectSellerResponse>;
    public sealed record RejectSellerResponse(Guid Id, string Status, string? RejectReason, DateTime ReviewAt);
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

            try
            {
                profile.Reject(request.AdminUserId, request.Reason);
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessRuleException(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new SellerRejectedEvent
            {
                SellerProfileId = profile.Id,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RejectReason = profile.RejectReason!,
                SourceService = "UserService",
                CorrelationId = Guid.NewGuid()
            }, cancellationToken);

            return new RejectSellerResponse(profile.Id, profile.Status, profile.RejectReason!, profile.ReviewedAt!.Value);
        }
    }
}
