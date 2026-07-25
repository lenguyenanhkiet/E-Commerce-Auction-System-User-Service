using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.Users.DeleteUser;
using ECommerceAuction.UserService.Application.UnitTests.Common;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.Users;

public sealed class DeleteUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserService _currentUser = Fakes.CurrentUser(Guid.NewGuid());
    private readonly IUnitOfWork _unitOfWork = Fakes.UnitOfWork();
    private readonly MassTransit.IPublishEndpoint _publish = Fakes.PublishEndpoint();

    private DeleteUserCommandHandler CreateSut() =>
        new(_userRepository, _currentUser, _unitOfWork, _publish);

    private static User CreateUser() =>
        User.CreateByAdmin("victim@test.local", "HASH", "Victim", "0900000000", null, null);

    [Fact]
    public async Task Handle_SoftDeletesUserAndWritesAudit()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await CreateSut().Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        Assert.NotNull(user.DeletedAt);
        await _userRepository.Received(1).AddAuditLogAsync(
            Arg.Is<UserAuditLog>(log =>
                log.Action == UserAuditActions.UserDeleted && log.TargetUserId == user.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotFoundOrAlreadyDeleted()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().Handle(new DeleteUserCommand(id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenAdminDeletesOwnAccount()
    {
        var user = CreateUser();
        var currentUser = Fakes.CurrentUser(user.Id);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var sut = new DeleteUserCommandHandler(_userRepository, currentUser, _unitOfWork, _publish);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.Handle(new DeleteUserCommand(user.Id), CancellationToken.None));
        Assert.Null(user.DeletedAt);
    }

    [Fact]
    public async Task Handle_Throws_WhenNotAuthenticated()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns((Guid?)null);
        var sut = new DeleteUserCommandHandler(_userRepository, currentUser, _unitOfWork, _publish);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None));
    }
}