using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.DeleteUser;

/// <summary>
/// Soft-deletes a user account by Admin.
/// </summary>
public sealed record DeleteUserCommand(Guid UserId) : ICommand;
