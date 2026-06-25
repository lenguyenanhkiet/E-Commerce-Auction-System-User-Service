namespace ECommerceAuction.UserService.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordResponse(
    string Email,
    string Status
);