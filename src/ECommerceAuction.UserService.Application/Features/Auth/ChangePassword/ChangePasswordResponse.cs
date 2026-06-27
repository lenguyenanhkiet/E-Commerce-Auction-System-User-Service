namespace ECommerceAuction.UserService.Application.Features.Users.ChangePassword;

public sealed record ChangePasswordResponse(
    Guid UserId,
    DateTime PasswordChangedAt
);