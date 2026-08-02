using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.Avatar;

public static class AvatarStorageKeyPolicy
{
    public static bool IsOwnedByUser(string? key, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(key) || userId == Guid.Empty)
        {
            return false;
        }

        var normalizedKey = key
            .Replace('\\', '/')
            .TrimStart('/');

        var expectedPrefix = $"user/avatar/{userId:D}/";

        return normalizedKey.StartsWith(
            expectedPrefix,
            StringComparison.OrdinalIgnoreCase);
    }
}