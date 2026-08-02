using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Users.Avatar.SetAvatar;

/// <summary>
/// Persists an already-uploaded avatar (URL + storage key) onto the authenticated user's profile.
/// The file upload itself is performed in the API layer via the shared Nexus.Upload library;
/// this command only records the result.
/// </summary>
public sealed record SetAvatarCommand(string AvatarUrl, string AvatarKey) : ICommand<SetAvatarResponse>;

/// <summary>
/// <paramref name="PreviousKey"/> is the storage key of the avatar that was just replaced (null if
/// the user had none). The API layer uses it to delete the now-orphaned old file from storage.
/// </summary>
public sealed record SetAvatarResponse(string AvatarUrl, string? PreviousKey);
