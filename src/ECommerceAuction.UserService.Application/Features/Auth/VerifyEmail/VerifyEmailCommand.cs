using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;

/// <summary>
<<<<<<< HEAD
/// Verifies the OTP sent during registration and creates the SQL user.
=======
///Pass - VerifyEmail: verifies the OTP sent during registration and creates the SQL user.
>>>>>>> develop
/// </summary>
public sealed record VerifyEmailCommand(
    string Email,
    string OtpCode) : ICommand<Guid>;
