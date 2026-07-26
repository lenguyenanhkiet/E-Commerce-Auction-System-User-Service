using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Nexus.Contracts.Events.Seller;
using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.ApproveSeller;

public sealed class ApproveSellerCommandHandler : ICommandHandler<ApproveSellerCommand, ApproveSellerResponse>
{
    private const string SellerRoleCode = "SELLER";

    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleManagementRepository _roleManagementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IReputationAwardService _reputationAwardService;

    public ApproveSellerCommandHandler(
        ISellerProfileRepository sellerProfileRepository,
        IUserRepository userRepository,
        IRoleManagementRepository roleManagementRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IBuyerVerificationRepository buyerVerificationRepository,
        IReputationAwardService reputationAwardService)
    {
        _sellerProfileRepository = sellerProfileRepository;
        _userRepository = userRepository;
        _roleManagementRepository = roleManagementRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
    }

    public async Task<ApproveSellerResponse> Handle(
        ApproveSellerCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(request.SellerProfileId, cancellationToken)
            ?? throw new NotFoundException("Seller application not found.");

        var user = await _userRepository.GetByIdAsync(profile.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        try
        {
            profile.Approve(request.AdminUserId);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var sellerRole = await _roleManagementRepository.QueryRoles()
            .FirstOrDefaultAsync(r => r.Code == SellerRoleCode, cancellationToken)
            ?? throw new BusinessRuleException($"Role '{SellerRoleCode}' is not configured.");

        user.AssignRole(UserRole.Assign(user.Id, sellerRole.Id, request.AdminUserId));

        var occurredAt = DateTimeOffset.UtcNow;
        var verificationProfile = await _buyerVerificationRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        if (verificationProfile is null)
        {
            verificationProfile = BuyerVerificationProfile.Create(user.Id, occurredAt);
            await _buyerVerificationRepository.AddAsync(verificationProfile, cancellationToken);
        }

        if (verificationProfile.VerifyPaymentMethod(occurredAt))
        {
            await _reputationAwardService.AwardConfirmedAsync(
                user.Id,
                ReputationEntryTypes.ProfileVerification,
                ReputationReasons.PaymentMethodVerified,
                BuyerReputationPoints.PaymentMethodVerified,
                "SELLER_PAYMENT_METHOD",
                profile.Id.ToString(),
                $"user:{user.Id}:payment-method-verified:v1",
                occurredAt,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new SellerApprovedEvent
        {
            SellerProfileId = profile.Id,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            SourceService = "UserService",
            CorrelationId = Guid.NewGuid()
        }, cancellationToken);

        return new ApproveSellerResponse(profile.Id, profile.Status, profile.ReviewedAt!.Value);
    }
}
