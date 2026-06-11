namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public record CheckLocalAccountResponse(
	Guid UserId,
	string Email,
	string? PhoneNumber,
	string FullName,
	string Status,
	string AccessToken
);