using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogs;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.AuditLogs;

public sealed class GetAuditLogsQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();

    private GetAuditLogsQueryHandler CreateSut() => new(_repository);

    private static UserAuditLog SampleLog() =>
        UserAuditLog.Create(
            actorUserId: Guid.NewGuid(),
            targetUserId: Guid.NewGuid(),
            action: UserAuditActions.UserCreated,
            entityType: nameof(User),
            entityId: Guid.NewGuid().ToString());

    [Fact]
    public async Task Handle_ReturnsMappedItems_AndComputesTotalPages()
    {
        var logs = new List<UserAuditLog> { SampleLog(), SampleLog() };
        _repository.GetPagedAsync(
                Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), 1, 20, Arg.Any<CancellationToken>())
            .Returns((logs, 25));

        var result = await CreateSut().Handle(new GetAuditLogsQuery(Page: 1, PageSize: 20), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.TotalPages); // ceil(25/20)
        Assert.Equal(UserAuditActions.UserCreated, result.Items[0].Action);
    }

    [Fact]
    public async Task Handle_ForwardsFiltersToRepository()
    {
        var actorId = Guid.NewGuid();
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.GetPagedAsync(
                Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<UserAuditLog>(), 0));

        await CreateSut().Handle(
            new GetAuditLogsQuery(Action: "USER_CREATED", ActorUserId: actorId, EntityType: "User",
                FromUtc: from, ToUtc: to, Page: 1, PageSize: 10),
            CancellationToken.None);

        await _repository.Received(1).GetPagedAsync(
            "USER_CREATED", actorId, null, "User", from, to, 1, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZeroTotalPages_WhenEmpty()
    {
        _repository.GetPagedAsync(
                Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<UserAuditLog>(), 0));

        var result = await CreateSut().Handle(new GetAuditLogsQuery(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Handle_Throws_WhenPagingInvalid(int page, int pageSize)
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().Handle(new GetAuditLogsQuery(Page: page, PageSize: pageSize), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenFromIsAfterTo()
    {
        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().Handle(new GetAuditLogsQuery(FromUtc: from, ToUtc: to), CancellationToken.None));
    }
}
