using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.Seller.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.RegisterSeller;

/// <summary>
/// Validates eligibility (email verified, reputation threshold, no active application),
/// creates the SellerProfile as Pending, and publishes SellerRegistrationSubmittedEvent.
/// </summary>
public sealed class RegisterSellerCommandHandler : ICommandHandler<RegisterSellerCommand, RegisterSellerResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;

    public RegisterSellerCommandHandler(ICurrentUserService currentUserService, ISellerProfileRepository sellerProfileRepository, IBuyerVerificationRepository buyerVerificationRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _sellerProfileRepository = sellerProfileRepository;
        _buyerVerificationRepository = buyerVerificationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
    }

    public async Task<RegisterSellerResponse> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
    {
        // User exists
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information was not found in JWT.");
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");

        var verificationProfile = await _buyerVerificationRepository.GetByUserIdAsync(userId, cancellationToken) ?? throw new BusinessRuleException("Buyer verification profile was not found.");

        ValidateEligibility(verificationProfile);

        // Check if you are already a seller.
        if (await _sellerProfileRepository.HasActiveApplicationAsync(user.Id, cancellationToken))
        {
            throw new ConflictException("User already has an active seller application.");
        }

        ValidateRequest(request);
        var now = _timeProvider.GetUtcNow();

        var sellerProfile = SellerProfile.Create
            (
                userId: userId,
                sellerType: request.SellerType,
                businessName: request.BusinessName,
                contactPhoneNumber: request.ContactPhoneNumber,
                taxCode: request.TaxCode,
                businessLicenseUrl: request.BusinessLicenseUrl,
                address: request.Address,
                bankAccountNumber: request.BankAccountNumber,
                bankName: request.BankName,
                bankAccountHolder: request.BankAccountHolder,
                submittedAt: now
            );
        await _sellerProfileRepository.AddAsync(sellerProfile, cancellationToken);
        await _publishEndpoint.Publish(
            new SellerEligibilityChanged(
                NewId.NextGuid(),
                now,
                user.Id,
                Found: true,
                UserStatus: user.Status,
                Deleted: user.DeletedAt is not null,
                SellerRoleActive: false,
                CanSell: false,
                EligibilityStatus: "INELIGIBLE",
                ReasonCode: "SELLER_APPLICATION_PENDING",
                SourceVersion: now.UtcTicks),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterSellerResponse(
        sellerProfile.Id,
        sellerProfile.Status,
        sellerProfile.ContactPhoneNumber,
        sellerProfile.SubmittedAt);
    }

    private static void ValidateEligibility(
        BuyerVerificationProfile verificationProfile)
    {
        if (!verificationProfile.IsEmailVerified)
        {
            throw new BusinessRuleException(
                "User must verify email before registering as a seller.");
        }

        if (!verificationProfile.IsPhoneVerified)
        {
            throw new BusinessRuleException(
                "User must verify phone number before registering as a seller.");
        }

        if (!verificationProfile.IsIdentityVerified)
        {
            throw new BusinessRuleException(
                "User must complete identity verification before registering as a seller.");
        }
    }

    private static void ValidateRequest(
        RegisterSellerCommand request)
    {
        if (!SellerTypes.IsValid(request.SellerType))
        {
            throw new BusinessRuleException(
                "Invalid seller type. Must be 'Individual' or 'Business'.");
        }

        if (string.IsNullOrWhiteSpace(request.BusinessName))
        {
            throw new BusinessRuleException(
                "Shop or business name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.ContactPhoneNumber))
        {
            throw new BusinessRuleException(
                "Seller contact phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TaxCode))
        {
            throw new BusinessRuleException(
                "Tax code is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.BusinessLicenseUrl))
        {
            throw new BusinessRuleException(
                "Business license is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            throw new BusinessRuleException(
                "Warehouse address is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.BankAccountNumber) ||
            string.IsNullOrWhiteSpace(request.BankName) ||
            string.IsNullOrWhiteSpace(
                request.BankAccountHolder))
        {
            throw new BusinessRuleException(
                "Settlement bank information is required.");
        }
    }
}
