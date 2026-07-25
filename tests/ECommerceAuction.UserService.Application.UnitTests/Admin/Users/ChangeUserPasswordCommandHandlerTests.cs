using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.Users.ChangeUserPassword;
using ECommerceAuction.UserService.Application.UnitTests.Common;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.Users;

public sealed class ChangeUserPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    private readonly IUserPasswordHistoryRepository _historyRepository =
        Substitute.For<IUserPasswordHistoryRepository>();

    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserService _currentUser = Fakes.CurrentUser(Guid.NewGuid());
    private readonly IUnitOfWork _unitOfWork = Fakes.UnitOfWork();
    private readonly MassTransit.IPublishEndpoint _publish = Fakes.PublishEndpoint();

    private ChangeUserPasswordCommandHandler CreateSut() =>
        new(_userRepository, _historyRepository, _passwordHasher, _currentUser, _unitOfWork, _publish);

    private static User CreateUser() =>
        User.CreateByAdmin("user@test.local", "OLDHASH", "Test User", "0900000000", null, null);

    [Fact]
    public async Task Handle_UpdatesPassword_StoresHistory_AndForcesChange_ByDefault()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword("NewPass@123").Returns("NEWHASH");

        var result = await CreateSut().Handle(
            new ChangeUserPasswordCommand(user.Id, "NewPass@123"), CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.True(user.MustChangePassword);
        await _historyRepository.Received(1).AddAsync(
            Arg.Is<UserPasswordHistory>(entry => entry.UserId == user.Id && entry.PasswordHash == "NEWHASH"),
            Arg.Any<CancellationToken>());
        await _historyRepository.Received(1).PruneOldEntriesAsync(
            user.Id, Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAuditLogAsync(
            Arg.Is<UserAuditLog>(log => log.Action == UserAuditActions.UserPasswordChangedByAdmin),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotForceChange_WhenRequireChangeIsFalse()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("NEWHASH");

        await CreateSut().Handle(
            new ChangeUserPasswordCommand(user.Id, "NewPass@123", RequireChangeOnNextLogin: false),
            CancellationToken.None);

        Assert.False(user.MustChangePassword);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotFound()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().Handle(new ChangeUserPasswordCommand(id, "NewPass@123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenNotAuthenticated()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns((Guid?)null);
        var sut = new ChangeUserPasswordCommandHandler(
            _userRepository, _historyRepository, _passwordHasher, currentUser, _unitOfWork, _publish);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.Handle(new ChangeUserPasswordCommand(Guid.NewGuid(), "NewPass@123"), CancellationToken.None));
    }
}