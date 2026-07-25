using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Nexus.Contracts.Events.Seller;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Sellers.RegisterSeller
{
    /// <summary>
    /// Validates eligibility (email verified, reputation threshold, no active application),
    /// creates the SellerProfile as Pending, and publishes SellerRegistrationSubmittedEvent.
    /// </summary>
    public sealed class RegisterSellerCommandHandler : ICommandHandler<RegisterSellerCommand, RegisterSellerResponse>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIdentityVerificationRepository _identityVerificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterSellerCommandHandler
            (ISellerProfileRepository sellerProfileRepository,
            IUserRepository userRepository,
            IIdentityVerificationRepository identityVerificationRepository,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint)
        {
            _sellerProfileRepository = sellerProfileRepository;
            _userRepository = userRepository;
            _identityVerificationRepository = identityVerificationRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint= publishEndpoint;
        }

        public async Task<RegisterSellerResponse> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
        {
            // User exists
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new NotFoundException ("User not found.");
            // Verified email
            if (!user.IsEmailConfirmed)
            {
                throw new BusinessRuleException("User must have a verified email to register as a seller.");
            }
            // Verified phone number
            if (!user.IsPhoneConfirmed)
            {
                throw new BusinessRuleException("User must have a verified phone number to register as a seller.");
            }
            // Identity must already be verified via POST /api/v1/identity-verifications.
            var identityVerification = await _identityVerificationRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (identityVerification is null || identityVerification.Status != IdentityVerificationState.Verified)
            {
                throw new BusinessRuleException("User must complete identity verification before registering as a seller.");
            }
            // Check if you are already a seller.
            if (await _sellerProfileRepository.HasActiveApplicationAsync(user.Id, cancellationToken))
            {
                throw new ConflictException("User already has an active seller application.");
            }

            ValidateSellerTypeRequirements(request);

            var sellerProfile = new SellerProfile(
                request.UserId,
                request.SellerType,
                request.BusinessName,
                request.TaxCode,
                request.BusinessLicenseUrl,
                request.Address,
                request.BankAccountNumber,
                request.BankName,
                request.BankAccountHolder
            );
            await _sellerProfileRepository.AddAsync(sellerProfile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var correlationId = Guid.NewGuid();
            await _publishEndpoint.Publish(new SellerRegistrationSubmittedEvent
            {
                SellerProfileId = sellerProfile.Id,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SellerType = sellerProfile.SellerType,
                SourceService = "UserService",
                CorrelationId = correlationId
            }, cancellationToken);
            return new RegisterSellerResponse(sellerProfile.Id, sellerProfile.Status, sellerProfile.SubmittedAt);
        }
        private static void ValidateSellerTypeRequirements(RegisterSellerCommand request)
        {
            if (request.SellerType == SellerType.Business)
            {
                if (string.IsNullOrWhiteSpace(request.BusinessName) ||
                    string.IsNullOrWhiteSpace(request.TaxCode) ||
                    string.IsNullOrWhiteSpace(request.BusinessLicenseUrl))
                {
                    throw new BusinessRuleException(
                        "Business sellers must provide business name, tax code, and business license.");
                }
            }
            else if (request.SellerType != SellerType.Individual && request.SellerType != SellerType.Business)
            {
                throw new BusinessRuleException("Invalid seller type. Must be 'Individual' or 'Business'.");
            }
            if (string.IsNullOrEmpty(request.BusinessName))
            {
                throw new BusinessRuleException("Business name is required.");
            }
            if (string.IsNullOrEmpty(request.BusinessLicenseUrl))
            {
                throw new BusinessRuleException("BusinessLicenseUrl is required.");
            }
            if (string.IsNullOrEmpty(request.TaxCode))
            {
                throw new BusinessRuleException("TaxCode is required.");
            }

        }

    }
}
