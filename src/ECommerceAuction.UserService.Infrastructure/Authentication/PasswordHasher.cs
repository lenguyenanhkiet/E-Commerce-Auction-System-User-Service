using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.AspNetCore.Identity;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
	private readonly PasswordHasher<object> _passwordHasher = new();

	public string HashPassword(string password)
	{
		return _passwordHasher.HashPassword(new object(), password);
	}

	public bool VerifyPassword(string password, string passwordHash)
	{
		var result = _passwordHasher.VerifyHashedPassword(
			new object(),
			passwordHash,
			password);

		return result == PasswordVerificationResult.Success;
	}
}