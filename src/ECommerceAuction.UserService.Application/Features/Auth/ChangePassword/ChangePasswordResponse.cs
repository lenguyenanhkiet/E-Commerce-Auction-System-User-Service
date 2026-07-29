namespace ECommerceAuction.UserService.Application.Features.Users.ChangePassword;

public sealed record ChangePasswordResponse(
    Guid UserId,
    DateTimeOffset ChangedAt
);