namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

public record RegisterAccountResponse(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string FullName,
    string Status
);