using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword
) : ICommand<ResetPasswordResponse>;