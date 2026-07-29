namespace ECommerceAuction.UserService.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordResponse(
    Guid UserId,
    DateTimeOffset PasswordChangedAt
);