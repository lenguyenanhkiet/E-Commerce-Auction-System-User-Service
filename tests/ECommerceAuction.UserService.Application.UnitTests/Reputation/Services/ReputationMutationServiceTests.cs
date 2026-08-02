using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Services;

public sealed class ReputationMutationServiceTests
{
    private static readonly DateTimeOffset OccurredAt =
        new(2026, 7, 31, 3, 0, 0, TimeSpan.Zero);

    private readonly IBuyerReputationRepository _buyerRepository =
        Substitute.For<IBuyerReputationRepository>();
    private readonly ISellerReputationRepository _sellerRepository =
        Substitute.For<ISellerReputationRepository>();
    private readonly IReputationLedgerRepository _ledgerRepository =
        Substitute.For<IReputationLedgerRepository>();

    [Theory]
    [InlineData(5, 5)]
    [InlineData(-3, -3)]
    public async Task ApplyAsync_AppliesSignedBuyerMutationAndAddsOneLedger(
        int delta,
        int expectedScore)
    {
        var userId = Guid.NewGuid();
        var profile = BuyerReputationProfile.Create(userId, OccurredAt);
        _buyerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);
        var service = CreateService();

        var result = await service.ApplyAsync(
            CreateMutation(userId, ReputationRoles.Buyer, delta));

        Assert.True(result.Applied);
        Assert.False(result.Duplicate);
        Assert.Equal(0, result.ScoreBefore);
        Assert.Equal(expectedScore, result.ScoreAfter);
        Assert.Equal(expectedScore, profile.ConfirmedScore);
        await _ledgerRepository.Received(1).AddAsync(
            Arg.Is<ReputationLedgerEntry>(entry =>
                entry.Role == ReputationRoles.Buyer &&
                entry.ScoreBefore == 0 &&
                entry.ScoreAfter == expectedScore &&
                entry.ScoreDelta == delta),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(20, 20)]
    [InlineData(-7, -7)]
    public async Task ApplyAsync_AppliesSignedSellerMutationAndAddsOneLedger(
        int delta,
        int expectedScore)
    {
        var userId = Guid.NewGuid();
        var profile = SellerReputationProfile.Create(userId, OccurredAt);
        _sellerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);
        var service = CreateService();

        var result = await service.ApplyAsync(
            CreateMutation(
                userId,
                ReputationRoles.Seller,
                delta,
                ReputationReasonCatalog.SellerProfileApproved));

        Assert.True(result.Applied);
        Assert.Equal(expectedScore, profile.ConfirmedScore);
        Assert.Equal(ReputationRoles.Seller, result.Role);
        await _ledgerRepository.Received(1).AddAsync(
            Arg.Is<ReputationLedgerEntry>(entry =>
                entry.Role == ReputationRoles.Seller &&
                entry.ScoreDelta == delta),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_ReturnsDuplicateWithoutChangingProjection()
    {
        var mutation = CreateMutation(Guid.NewGuid(), ReputationRoles.Buyer, 1);
        _ledgerRepository.ExistsByIdempotencyKeyAsync(
                mutation.IdempotencyKey,
                Arg.Any<CancellationToken>())
            .Returns(true);
        var service = CreateService();

        var result = await service.ApplyAsync(mutation);

        Assert.False(result.Applied);
        Assert.True(result.Duplicate);
        await _buyerRepository.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, default);
        await _sellerRepository.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, default);
        await _ledgerRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
    }

    [Theory]
    [InlineData("ADMIN", "buyer.profile.email-verified", 1)]
    [InlineData("BUYER", "unknown.reason", 1)]
    [InlineData("BUYER", "buyer.profile.email-verified", 0)]
    public async Task ApplyAsync_RejectsInvalidMutation(
        string role,
        string reason,
        int delta)
    {
        var service = CreateService();
        var mutation = CreateMutation(Guid.NewGuid(), role, delta, reason);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.ApplyAsync(mutation));
    }

    [Theory]
    [InlineData("BUYER", "seller.profile.approved")]
    [InlineData("SELLER", "buyer.profile.email-verified")]
    public async Task ApplyAsync_RejectsReasonForDifferentRole(
        string role,
        string reason)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ApplyAsync(
                CreateMutation(Guid.NewGuid(), role, 1, reason)));
    }

    [Fact]
    public async Task ApplyAsync_NormalizesIdempotencyKeyBeforeDuplicateCheck()
    {
        _ledgerRepository.ExistsByIdempotencyKeyAsync(
                "mutation:normalized",
                Arg.Any<CancellationToken>())
            .Returns(true);
        var service = CreateService();

        var result = await service.ApplyAsync(
            CreateMutation(
                Guid.NewGuid(),
                ReputationRoles.Buyer,
                1,
                idempotencyKey: " mutation:normalized "));

        Assert.True(result.Duplicate);
    }

    [Fact]
    public async Task ApplyAsync_CreatesMissingProfilesAtZeroBeforeApplyingDelta()
    {
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var service = CreateService();

        var buyerResult = await service.ApplyAsync(
            CreateMutation(buyerId, ReputationRoles.Buyer, 2));
        var sellerResult = await service.ApplyAsync(
            CreateMutation(
                sellerId,
                ReputationRoles.Seller,
                20,
                ReputationReasonCatalog.SellerProfileApproved));

        Assert.Equal((0, 2), (buyerResult.ScoreBefore, buyerResult.ScoreAfter));
        Assert.Equal((0, 20), (sellerResult.ScoreBefore, sellerResult.ScoreAfter));
        await _buyerRepository.Received(1).AddAsync(
            Arg.Is<BuyerReputationProfile>(profile =>
                profile.UserId == buyerId && profile.ConfirmedScore == 2),
            Arg.Any<CancellationToken>());
        await _sellerRepository.Received(1).AddAsync(
            Arg.Is<SellerReputationProfile>(profile =>
                profile.UserId == sellerId && profile.ConfirmedScore == 20),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_KeepsBuyerAndSellerScoresIndependent()
    {
        var userId = Guid.NewGuid();
        var buyer = BuyerReputationProfile.Create(userId, OccurredAt);
        var seller = SellerReputationProfile.Create(userId, OccurredAt);
        _buyerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(buyer);
        _sellerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(seller);
        var service = CreateService();

        await service.ApplyAsync(
            CreateMutation(userId, ReputationRoles.Buyer, 3));
        await service.ApplyAsync(
            CreateMutation(
                userId,
                ReputationRoles.Seller,
                -4,
                ReputationReasonCatalog.SellerAuctionShillBiddingConfirmed));

        Assert.Equal(3, buyer.ConfirmedScore);
        Assert.Equal(-4, seller.ConfirmedScore);
    }

    [Fact]
    public async Task ApplyAsync_StagesChangesWithoutCommitOrPublisherDependencies()
    {
        var service = new ReputationMutationService(
            _buyerRepository,
            _sellerRepository,
            _ledgerRepository);

        var result = await service.ApplyAsync(
            CreateMutation(Guid.NewGuid(), ReputationRoles.Buyer, 1));

        Assert.True(result.Applied);
    }

    [Fact]
    public async Task ReverseAsync_AppliesCompensatingDeltaWithoutChangingOriginal()
    {
        var userId = Guid.NewGuid();
        var original = ReputationLedgerEntry.Create(
            CreateMutation(userId, ReputationRoles.Buyer, 5),
            0,
            5,
            OccurredAt);
        var originalSnapshot = (
            original.ScoreDelta,
            original.ScoreBefore,
            original.ScoreAfter,
            original.ReversesEntryId);
        var profile = BuyerReputationProfile.Create(userId, OccurredAt);
        profile.ApplyConfirmedDelta(12, OccurredAt);
        _ledgerRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(original);
        _buyerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);
        var service = CreateService();

        var result = await service.ReverseAsync(
            original.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "reversal:buyer:1",
            "support-case-1",
            OccurredAt.AddDays(1));

        Assert.True(result.Applied);
        Assert.Equal((12, 7), (result.ScoreBefore, result.ScoreAfter));
        Assert.Equal(7, profile.ConfirmedScore);
        Assert.Equal(
            originalSnapshot,
            (
                original.ScoreDelta,
                original.ScoreBefore,
                original.ScoreAfter,
                original.ReversesEntryId));
        await _ledgerRepository.Received(1).AddAsync(
            Arg.Is<ReputationLedgerEntry>(entry =>
                entry.ReversesEntryId == original.Id &&
                entry.ScoreDelta == -5 &&
                entry.ScoreBefore == 12 &&
                entry.ScoreAfter == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReverseAsync_UsesSellerProjection()
    {
        var userId = Guid.NewGuid();
        var original = ReputationLedgerEntry.Create(
            CreateMutation(
                userId,
                ReputationRoles.Seller,
                -7,
                ReputationReasonCatalog.SellerAuctionShillBiddingConfirmed),
            20,
            13,
            OccurredAt);
        var profile = SellerReputationProfile.Create(userId, OccurredAt);
        profile.ApplyConfirmedDelta(30, OccurredAt);
        _ledgerRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(original);
        _sellerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);
        var service = CreateService();

        var result = await service.ReverseAsync(
            original.Id,
            Guid.NewGuid(),
            null,
            "reversal:seller:1",
            "support-case-2",
            OccurredAt.AddDays(1));

        Assert.Equal((30, 37), (result.ScoreBefore, result.ScoreAfter));
        Assert.Equal(37, profile.ConfirmedScore);
        Assert.Equal(ReputationRoles.Seller, result.Role);
    }

    [Fact]
    public async Task ReverseAsync_ThrowsWhenOriginalDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ReverseAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                "reversal:not-found",
                "support-case-3",
                OccurredAt));
    }

    [Fact]
    public async Task ReverseAsync_ReturnsDuplicateForRepeatedReversal()
    {
        var original = ReputationLedgerEntry.Create(
            CreateMutation(Guid.NewGuid(), ReputationRoles.Buyer, 5),
            0,
            5,
            OccurredAt);
        _ledgerRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(original);
        _ledgerRepository.HasReversalAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(true);
        var service = CreateService();

        var result = await service.ReverseAsync(
            original.Id,
            Guid.NewGuid(),
            null,
            "reversal:repeated",
            "support-case-4",
            OccurredAt);

        Assert.True(result.Duplicate);
        await _ledgerRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
    }

    [Fact]
    public async Task ReverseAsync_ReturnsDuplicateForExistingIdempotencyKey()
    {
        var original = ReputationLedgerEntry.Create(
            CreateMutation(Guid.NewGuid(), ReputationRoles.Buyer, 5),
            0,
            5,
            OccurredAt);
        _ledgerRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(original);
        _ledgerRepository.ExistsByIdempotencyKeyAsync(
                "reversal:duplicate-key",
                Arg.Any<CancellationToken>())
            .Returns(true);
        var service = CreateService();

        var result = await service.ReverseAsync(
            original.Id,
            Guid.NewGuid(),
            null,
            "reversal:duplicate-key",
            "support-case-5",
            OccurredAt);

        Assert.True(result.Duplicate);
    }

    [Fact]
    public async Task ReverseAsync_RejectsReversingAReversalEntry()
    {
        var original = ReputationLedgerEntry.Create(
            CreateMutation(Guid.NewGuid(), ReputationRoles.Buyer, 5),
            0,
            5,
            OccurredAt);
        var reversal = ReputationLedgerEntry.CreateReversal(
            original,
            5,
            Guid.NewGuid(),
            null,
            "reversal:first",
            "support-case-6",
            OccurredAt,
            OccurredAt);
        _ledgerRepository.GetByIdAsync(reversal.Id, Arg.Any<CancellationToken>())
            .Returns(reversal);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReverseAsync(
                reversal.Id,
                Guid.NewGuid(),
                null,
                "reversal:second",
                "support-case-7",
                OccurredAt));
    }

    [Fact]
    public async Task ReverseAsync_ThrowsOverflowForMinimumOriginalDelta()
    {
        var userId = Guid.NewGuid();
        var original = ReputationLedgerEntry.Create(
            CreateMutation(userId, ReputationRoles.Buyer, int.MinValue),
            0,
            int.MinValue,
            OccurredAt);
        var profile = BuyerReputationProfile.Create(userId, OccurredAt);
        _ledgerRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>())
            .Returns(original);
        _buyerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);
        var service = CreateService();

        await Assert.ThrowsAsync<OverflowException>(() =>
            service.ReverseAsync(
                original.Id,
                Guid.NewGuid(),
                null,
                "reversal:overflow",
                "support-case-8",
                OccurredAt));

        Assert.Equal(0, profile.ConfirmedScore);
    }

    [Fact]
    public async Task ReputationAwardService_DelegatesLegacyAwardToMutationService()
    {
        var userId = Guid.NewGuid();
        var mutationService = Substitute.For<IReputationMutationService>();
        mutationService.ApplyAsync(
                Arg.Any<ReputationMutation>(),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.AppliedResult(
                Guid.NewGuid(),
                ReputationRoles.Buyer,
                0,
                1,
                "BASIC",
                "BASIC"));
        var service = new ReputationAwardService(mutationService);

        var applied = await service.AwardConfirmedAsync(
            userId,
            ReputationEntryTypes.ProfileVerification,
            ReputationReasons.EmailVerified,
            BuyerReputationPoints.EmailVerified,
            "USER",
            userId.ToString(),
            $"buyer:{userId}:profile:email-verified",
            OccurredAt);

        Assert.True(applied);
        await mutationService.Received(1).ApplyAsync(
            Arg.Is<ReputationMutation>(mutation =>
                mutation.UserId == userId &&
                mutation.Role == ReputationRoles.Buyer &&
                mutation.ReasonCode ==
                    ReputationReasonCatalog.BuyerProfileEmailVerified &&
                mutation.ScoreDelta == BuyerReputationPoints.EmailVerified &&
                mutation.IdempotencyKey ==
                    $"buyer:{userId}:profile:email-verified" &&
                mutation.MessageId != Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReputationAwardService_ReturnsFalseForDuplicateMutation()
    {
        var mutationService = Substitute.For<IReputationMutationService>();
        mutationService.ApplyAsync(
                Arg.Any<ReputationMutation>(),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.DuplicateResult());
        var service = new ReputationAwardService(mutationService);

        var applied = await service.AwardConfirmedAsync(
            Guid.NewGuid(),
            ReputationEntryTypes.ProfileVerification,
            ReputationReasons.AddressVerified,
            BuyerReputationPoints.AddressVerified,
            "USER_ADDRESS",
            Guid.NewGuid().ToString(),
            "buyer:address:duplicate",
            OccurredAt);

        Assert.False(applied);
    }

    [Fact]
    public async Task ReputationAwardService_RejectsInvalidLegacyEntryType()
    {
        var service = new ReputationAwardService(
            Substitute.For<IReputationMutationService>());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AwardConfirmedAsync(
                Guid.NewGuid(),
                "UNKNOWN",
                ReputationReasons.EmailVerified,
                1,
                "USER",
                "user-1",
                "buyer:user-1:profile:email-verified",
                OccurredAt));
    }

    [Fact]
    public async Task ReputationAwardService_CreatesDeterministicMessageId()
    {
        var mutations = new List<ReputationMutation>();
        var mutationService = Substitute.For<IReputationMutationService>();
        mutationService.ApplyAsync(
                Arg.Do<ReputationMutation>(mutations.Add),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.AppliedResult(
                Guid.NewGuid(),
                ReputationRoles.Buyer,
                0,
                1,
                "BASIC",
                "BASIC"));
        var service = new ReputationAwardService(mutationService);

        for (var index = 0; index < 2; index++)
        {
            await service.AwardConfirmedAsync(
                Guid.NewGuid(),
                ReputationEntryTypes.ProfileVerification,
                ReputationReasons.EmailVerified,
                1,
                "USER",
                "user-1",
                " buyer:user-1:profile:email-verified ",
                OccurredAt);
        }

        Assert.Equal(2, mutations.Count);
        Assert.Equal(mutations[0].MessageId, mutations[1].MessageId);
        Assert.Equal(
            "buyer:user-1:profile:email-verified",
            mutations[0].IdempotencyKey);
    }

    [Fact]
    public void AddApplication_RegistersMutationServiceOnce()
    {
        var services = new ServiceCollection();

        ECommerceAuction.UserService.Application.DependencyInjection
            .AddApplication(services);

        var registrations = services.Where(
            descriptor =>
                descriptor.ServiceType == typeof(IReputationMutationService));
        var registration = Assert.Single(registrations);
        Assert.Equal(
            typeof(ReputationMutationService),
            registration.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    private ReputationMutationService CreateService() =>
        new(_buyerRepository, _sellerRepository, _ledgerRepository);

    private static ReputationMutation CreateMutation(
        Guid userId,
        string role,
        int delta,
        string reason = ReputationReasonCatalog.BuyerProfileEmailVerified,
        string? idempotencyKey = null) =>
        new(
            userId,
            role,
            reason,
            delta,
            "user-service",
            "TEST",
            userId.ToString(),
            idempotencyKey ?? $"mutation:{Guid.NewGuid():N}",
            "REPUTATION_V1",
            Guid.NewGuid(),
            null,
            null,
            OccurredAt);
}
