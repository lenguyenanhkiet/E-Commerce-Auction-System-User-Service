using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class OrderViolationConfirmedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<OrderViolationConfirmed>
{
    private static readonly IReadOnlyDictionary<string, int> SellerPenalties =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [ReputationReasonCatalog.SellerShipmentLate] = -3,
            [ReputationReasonCatalog.SellerSevereViolation] = -7,
            [ReputationReasonCatalog.SellerOutOfStock] = -10,
            [ReputationReasonCatalog.SellerWrongPrice] = -15,
            [ReputationReasonCatalog.SellerNotFulfilled] = -25,
            [ReputationReasonCatalog.SellerNotAsDescribed] = -20,
            [ReputationReasonCatalog.SellerImportantInformationMissing] = -12,
            [ReputationReasonCatalog.SellerFakeTracking] = -40,
            [ReputationReasonCatalog.SellerFaultDispute] = -15,
            [ReputationReasonCatalog.SellerCounterfeit] = -100,
            [ReputationReasonCatalog.SellerFraud] = -200,
            [ReputationReasonCatalog.SellerAdminCancelled] = -30
        };

    public Task Consume(ConsumeContext<OrderViolationConfirmed> context)
    {
        var m = context.Message;
        var buyerFault = m.FaultParty.Equals("BUYER", StringComparison.OrdinalIgnoreCase);
        var reason = buyerFault ? ReputationReasonCatalog.BuyerMaliciousReturn : m.ReasonCode;
        var delta = buyerFault
            ? -ReputationScoreCalculator.CalculateTransactionValue(m.TransactionAmount)
            : SellerPenalties.TryGetValue(reason, out var penalty)
                ? penalty
                : throw new ArgumentException($"Unsupported order violation '{reason}'.");

        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(OrderViolationConfirmed), m.OrderId.ToString("N"),
            buyerFault ? m.BuyerId : m.SellerId,
            buyerFault ? ReputationRoles.Buyer : ReputationRoles.Seller,
            context.CancellationToken,
            true,
            (reason, delta));
    }
}
