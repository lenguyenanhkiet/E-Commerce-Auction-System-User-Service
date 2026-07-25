using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Users.RequestPhoneOtp;
using ECommerceAuction.UserService.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.VerifyPhoneOtp
{
    public sealed class VerifyPhoneOtpCommandHandler : ICommandHandler<VerifyPhoneOtpCommand, VerifyPhoneOtpResponse>
    {
        private const int MaxAttempts = 5;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyPhoneOtpCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, ICacheService cacheService, IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<VerifyPhoneOtpResponse> Handle(VerifyPhoneOtpCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User information not found in JWT.");

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");
            if (user.IsPhoneConfirmed)
            {
                throw new BusinessRuleException("Phone number is already verified.");
            }

            var otpKey = RequestPhoneOtpCommandHandler.BuildOtpKey(user.Id);
            var otpModel = await _cacheService.GetAsync<PhoneOtpCacheModel>(otpKey, cancellationToken)
                ?? throw new BusinessRuleException("OTP has expired or was never requested. Please request a new one.");

            if (otpModel.AttemptCount >= MaxAttempts)
            {
                await _cacheService.RemoveAsync(otpKey, cancellationToken);
                throw new BusinessRuleException("Too many wrong attempts. Please request a new OTP.");
            }

            if (otpModel.OtpCode != request.OtpCode.Trim())
            {
                var remaining = otpModel.ExpiresAt - DateTime.UtcNow;
                await _cacheService.SetAsync(
                    otpKey,
                    otpModel with { AttemptCount = otpModel.AttemptCount + 1 },
                    remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(1),
                    cancellationToken);

                throw new BusinessRuleException("Incorrect OTP code.");
            }

            user.ConfirmPhoneChange();
            user.ReputationProfile?.AddPhoneVerificationPoint();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cacheService.RemoveAsync(otpKey, cancellationToken);

            return new VerifyPhoneOtpResponse(true, user.ReputationProfile?.Score ?? 0);
        }
    }
}
