using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;
using Nexus.Shared.Contracts.Events.User;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.RequestPhoneOtp
{
    public sealed class RequestPhoneOtpCommandHandler : ICommandHandler<RequestPhoneOtpCommand, RequestPhoneOtpResponse>
    {
        private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);

        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly IPublishEndpoint _publishEndpoint;

        public RequestPhoneOtpCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, ICacheService cacheService, IPublishEndpoint publishEndpoint)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _cacheService = cacheService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<RequestPhoneOtpResponse> Handle(RequestPhoneOtpCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User information not found in JWT.");

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                throw new BusinessRuleException("No phone number on this account. Update your profile first.");
            }

            if (user.IsPhoneConfirmed)
            {
                throw new BusinessRuleException("Phone number is already verified.");
            }

            // Block spam: Do not resend within 60s 
            var cooldownKey = BuildCooldownKey(user.Id);
            var isInCooldown = await _cacheService.GetAsync<string>(cooldownKey, cancellationToken);

            if (isInCooldown is not null)
            {
                throw new BusinessRuleException("Please wait before requesting another OTP.");
            }

            var otpModel = new PhoneOtpCacheModel
                (
                    UserId: user.Id,
                    OtpCode: GenerateOtpCode(),
                    PhoneNumber: user.PhoneNumber,
                    ExpiresAt: DateTime.UtcNow.Add(OtpExpiration),
                    AttemptCount: 0
                );

            await _cacheService.SetAsync(BuildOtpKey(user.Id), otpModel, OtpExpiration, cancellationToken);
            await _cacheService.SetAsync(cooldownKey, "1", ResendCooldown, cancellationToken);

            await _publishEndpoint.Publish(new UserVerificationSmsOtpRequestedEvent
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                OtpCode = otpModel.OtpCode,
                OtpExpiresAt = otpModel.ExpiresAt,
                SourceService = "UserService",
                CorrelationId = Guid.NewGuid()
            }, cancellationToken);
            return new RequestPhoneOtpResponse(MaskPhoneNumber(user.PhoneNumber), otpModel.ExpiresAt);
        }
        internal static string BuildOtpKey(Guid userId) => $"users:phone-otp:{userId}";
        internal static string BuildCooldownKey(Guid userId) => $"users:phone-otp-cooldown:{userId}";

        private static string GenerateOtpCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string MaskPhoneNumber(string phone)
        => phone.Length <= 4 ? phone : new string('*', phone.Length - 3) + phone[^3..];
    }
    public sealed record PhoneOtpCacheModel(
        Guid UserId,
        string PhoneNumber,
        string OtpCode,
        DateTime ExpiresAt,
        int AttemptCount);

    }


