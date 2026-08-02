using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Reputation.Ratings;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using MassTransit;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Ratings;

public sealed class SubmitRatingCommandHandlerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Valid_rating_is_persisted_published_and_saved_once(int score)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        var repository = Substitute.For<IReputationRatingRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var publisher = Substitute.For<IPublishEndpoint>();
        var rater = Guid.NewGuid();
        var target = Guid.NewGuid();
        var transaction = Guid.NewGuid();
        currentUser.UserId.Returns(rater);
        repository.GetEligibilityAsync("ORDER", transaction, rater, target, Arg.Any<CancellationToken>())
            .Returns(TransactionRatingEligibility.Create(
                "ORDER", transaction, rater, target,
                DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1)));
        repository.ExistsRatingAsync("ORDER", transaction, rater, target, Arg.Any<CancellationToken>())
            .Returns(false);
        var handler = new SubmitRatingCommandHandler(
            currentUser, repository, unitOfWork, publisher, TimeProvider.System);

        var result = await handler.Handle(
            new SubmitRatingCommand("ORDER", transaction, target, score, "Good"),
            CancellationToken.None);

        Assert.Equal(score, result.Score);
        await repository.Received(1).AddRatingAsync(
            Arg.Is<UserRating>(x => x.RaterUserId == rater && x.TargetUserId == target),
            CancellationToken.None);
        await publisher.ReceivedWithAnyArgs(1).Publish<object>(default!, default);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Score_outside_one_to_five_is_rejected(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UserRating.Create(
            "ORDER", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            score, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Self_rating_is_rejected()
    {
        var userId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => UserRating.Create(
            "ORDER", Guid.NewGuid(), userId, userId, 5, null, DateTimeOffset.UtcNow));
    }
}
