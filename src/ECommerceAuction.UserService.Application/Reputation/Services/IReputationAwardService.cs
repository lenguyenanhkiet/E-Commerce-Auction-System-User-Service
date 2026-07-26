using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Reputation.Services;

public interface IReputationAwardService
{
    Task<bool> AwardConfirmedAsync(
        Guid userId,
        string entryType,
        string reason,
        int points,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default);
}