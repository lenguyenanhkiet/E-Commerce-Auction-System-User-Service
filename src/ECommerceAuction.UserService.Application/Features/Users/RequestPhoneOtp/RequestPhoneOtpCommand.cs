using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.RequestPhoneOtp
{
    public sealed record RequestPhoneOtpCommand : ICommand<RequestPhoneOtpResponse>;
    public sealed record RequestPhoneOtpResponse(string PhoneNumber, DateTime OtpExpirationDate);


}
