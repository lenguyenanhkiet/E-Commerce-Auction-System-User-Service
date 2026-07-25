using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Users
{
    /// <summary>
    /// /// Stores the history of password hashes previously used by the user to support:
    /// 1. Preventing changes to passwords that match any of the last 3 passwords used
    /// 2. A background job that determines when a periodic password change is required
    /// </summary>
    public class UserPasswordHistory
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private UserPasswordHistory() { }
        public static UserPasswordHistory Create(Guid UserId, string PasswordHash)
        {
            return new UserPasswordHistory
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                PasswordHash = PasswordHash,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
