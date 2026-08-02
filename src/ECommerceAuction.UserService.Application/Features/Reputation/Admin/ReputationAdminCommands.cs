using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Admin;

public sealed record AdjustReputationCommand(
    Guid OperationId, Guid UserId, string Role, string Reason,
    int Delta, string EvidenceReference) : ICommand;
public sealed record ReverseReputationEntryCommand(
    Guid OperationId, Guid EntryId, Guid UserId, string EvidenceReference) : ICommand;
public sealed record ClearReputationRestrictionCommand(
    Guid OperationId, Guid UserId, string Role, string EvidenceReference) : ICommand;

public sealed class AdjustReputationCommandHandler(
    IReputationMutationService mutations, IUnitOfWork unitOfWork,
    IPublishEndpoint publisher, TimeProvider timeProvider) : ICommandHandler<AdjustReputationCommand>
{
    public async Task Handle(AdjustReputationCommand request, CancellationToken cancellationToken)
    {
        if (!AdminReputationReasonCatalog.IsAllowed(request.Reason))
            throw new ArgumentException("Invalid admin reputation reason.");
        if (request.Delta == 0) throw new ArgumentOutOfRangeException(nameof(request.Delta));
        if (string.IsNullOrWhiteSpace(request.EvidenceReference))
            throw new ArgumentException("Evidence reference is required.");
        if (!ReputationRoles.IsValid(request.Role)) throw new ArgumentException("Invalid role.");

        var now = timeProvider.GetUtcNow();
        var reason = request.Role == ReputationRoles.Buyer
            ? ReputationReasonCatalog.BuyerAdminAdjustment
            : ReputationReasonCatalog.SellerAdminAdjustment;
        var result = await mutations.ApplyAsync(new ReputationMutation(
            request.UserId, request.Role, reason, request.Delta, "user-service",
            nameof(AdjustReputationCommand), request.OperationId.ToString("N"),
            $"admin-adjustment:{request.OperationId:N}", "REPUTATION_V1",
            request.OperationId, request.OperationId, request.EvidenceReference, now), cancellationToken);
        if (result.Applied)
            await publisher.Publish(new ReputationScoreUpdated(Guid.NewGuid(), request.OperationId, now,
                request.UserId, request.Role, result.EntryId!.Value, reason,
                result.ScoreBefore!.Value, result.ScoreAfter!.Value), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ReverseReputationEntryCommandHandler(
    IReputationMutationService mutations, IUnitOfWork unitOfWork,
    IPublishEndpoint publisher, TimeProvider timeProvider) : ICommandHandler<ReverseReputationEntryCommand>
{
    public async Task Handle(ReverseReputationEntryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceReference))
            throw new ArgumentException("Evidence reference is required.");
        var now = timeProvider.GetUtcNow();
        var result = await mutations.ReverseAsync(request.EntryId, request.OperationId, request.OperationId,
            $"admin-reversal:{request.OperationId:N}", request.EvidenceReference, now, cancellationToken);
        if (result.Applied)
            await publisher.Publish(new ReputationScoreUpdated(Guid.NewGuid(), request.OperationId, now,
                request.UserId, result.Role!, result.EntryId!.Value, "reversal",
                result.ScoreBefore!.Value, result.ScoreAfter!.Value), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ClearReputationRestrictionCommandHandler(
    IBuyerReputationRepository buyers, ISellerReputationRepository sellers,
    IUnitOfWork unitOfWork, IPublishEndpoint publisher,
    TimeProvider timeProvider) : ICommandHandler<ClearReputationRestrictionCommand>
{
    public async Task Handle(ClearReputationRestrictionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceReference))
            throw new ArgumentException("Evidence reference is required.");
        var now = timeProvider.GetUtcNow();
        if (request.Role == ReputationRoles.Buyer)
            (await buyers.GetByUserIdAsync(request.UserId, cancellationToken)
             ?? throw new KeyNotFoundException()).ClearAuctionRestriction(now);
        else if (request.Role == ReputationRoles.Seller)
            (await sellers.GetByUserIdAsync(request.UserId, cancellationToken)
             ?? throw new KeyNotFoundException()).ClearRestrictions(now);
        else throw new ArgumentException("Invalid role.");

        await publisher.Publish(new ReputationRestrictionChanged(
            Guid.NewGuid(), request.OperationId, now, request.UserId,
            request.Role, "NONE", false), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
