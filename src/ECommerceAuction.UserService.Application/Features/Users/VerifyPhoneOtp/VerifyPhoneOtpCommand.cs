using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.VerifyPhoneOtp
{
    public sealed record VerifyPhoneOtpCommand(string OtpCode) : ICommand<VerifyPhoneOtpResponse>;
    public sealed record VerifyPhoneOtpResponse(bool IsVerified, int ReputationScore);
}
