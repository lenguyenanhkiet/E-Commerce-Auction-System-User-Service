using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email
) : ICommand<ForgotPasswordResponse>;