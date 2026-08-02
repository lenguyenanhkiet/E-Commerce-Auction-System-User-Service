using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Admin;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using MassTransit;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Admin;

public sealed class ReputationAdminCommandTests
{
    [Theory]
    [InlineData("FREE_TEXT", 10, "ticket-1")]
    [InlineData("ADMIN_CORRECTION", 0, "ticket-1")]
    [InlineData("ADMIN_CORRECTION", 10, "")]
    public async Task Invalid_adjustment_is_rejected(string reason, int delta, string evidence)
    {
        var handler = CreateAdjustmentHandler();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => handler.Handle(
            new AdjustReputationCommand(Guid.NewGuid(), Guid.NewGuid(), ReputationRoles.Buyer,
                reason, delta, evidence), CancellationToken.None));
    }

    [Fact]
    public async Task Valid_adjustment_uses_mutation_service_and_saves_once()
    {
        var mutations = Substitute.For<IReputationMutationService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        mutations.ApplyAsync(Arg.Any<ReputationMutation>(), Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.DuplicateResult());
        var handler = new AdjustReputationCommandHandler(
            mutations, unitOfWork, Substitute.For<IPublishEndpoint>(), TimeProvider.System);

        await handler.Handle(new AdjustReputationCommand(
            Guid.NewGuid(), Guid.NewGuid(), ReputationRoles.Buyer,
            "ADMIN_CORRECTION", 10, "ticket-1"), CancellationToken.None);

        await mutations.Received(1).ApplyAsync(
            Arg.Is<ReputationMutation>(x => x.ScoreDelta == 10 &&
                x.IdempotencyKey.StartsWith("admin-adjustment:")), CancellationToken.None);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    private static AdjustReputationCommandHandler CreateAdjustmentHandler() => new(
        Substitute.For<IReputationMutationService>(), Substitute.For<IUnitOfWork>(),
        Substitute.For<IPublishEndpoint>(), TimeProvider.System);
}
