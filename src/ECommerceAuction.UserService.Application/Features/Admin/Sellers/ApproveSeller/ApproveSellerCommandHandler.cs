using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Nexus.Contracts.Events.Seller;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.ApproveSeller;

public sealed class ApproveSellerCommandHandler : ICommandHandler<ApproveSellerCommand, ApproveSellerResponse>
{
    private const string SellerRoleCode = "SELLER";

    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleManagementRepository _roleManagementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApproveSellerCommandHandler(
        ISellerProfileRepository sellerProfileRepository,
        IUserRepository userRepository,
        IRoleManagementRepository roleManagementRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _sellerProfileRepository = sellerProfileRepository;
        _userRepository = userRepository;
        _roleManagementRepository = roleManagementRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
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

        if (user.ReputationProfile is null)
        {
            throw new BusinessRuleException("User does not have a reputation profile.");
        }

        user.ReputationProfile.AddVerifiedPaymentMethodPoint();
        user.ReputationProfile.AddTaxVerificationPoint();
        user.ReputationProfile.AddBusinessLicenseVerificationPoint();
        user.ReputationProfile.AddBusinessAddressVerificationPoint();

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