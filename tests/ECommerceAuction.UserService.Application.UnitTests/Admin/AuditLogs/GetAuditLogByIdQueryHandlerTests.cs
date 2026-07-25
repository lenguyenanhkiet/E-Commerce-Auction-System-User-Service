using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogById;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.AuditLogs;

public sealed class GetAuditLogByIdQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();

    private GetAuditLogByIdQueryHandler CreateSut() => new(_repository);

    [Fact]
    public async Task Handle_ReturnsMappedEntry_WhenFound()
    {
        var targetId = Guid.NewGuid();
        var log = UserAuditLog.Create(
            actorUserId: Guid.NewGuid(),
            targetUserId: targetId,
            action: UserAuditActions.UserDeleted,
            entityType: nameof(User),
            entityId: targetId.ToString());

        _repository.GetByIdAsync(log.Id, Arg.Any<CancellationToken>()).Returns(log);

        var result = await CreateSut().Handle(new GetAuditLogByIdQuery(log.Id), CancellationToken.None);

        Assert.Equal(log.Id, result.Id);
        Assert.Equal(UserAuditActions.UserDeleted, result.Action);
        Assert.Equal(targetId, result.TargetUserId);
    }

    [Fact]
    public async Task Handle_Throws_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((UserAuditLog?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().Handle(new GetAuditLogByIdQuery(id), CancellationToken.None));
    }
}
