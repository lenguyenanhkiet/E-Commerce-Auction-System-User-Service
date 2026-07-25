using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.Users.CreateUser;
using ECommerceAuction.UserService.Application.UnitTests.Common;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Reputation;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.Users;

public sealed class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserService _currentUser = Fakes.CurrentUser(Guid.NewGuid());
    private readonly IUnitOfWork _unitOfWork = Fakes.UnitOfWork();
    private readonly MassTransit.IPublishEndpoint _publish = Fakes.PublishEndpoint();
    private readonly IRoleManagementRepository _roleRepository;

    public CreateUserCommandHandlerTests()
    {
        (_roleRepository, _) = Fakes.RoleRepository();
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("HASHED");
        _userRepository.CheckEmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _userRepository.CheckPhoneExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
    }

    private CreateUserCommandHandler CreateSut() =>
        new(_userRepository, _roleRepository, _passwordHasher, _currentUser, _unitOfWork, _publish);

    private static CreateUserCommand Command(IReadOnlyList<string>? roleCodes = null) =>
        new("New@Test.Local", "0901234567", "New User", "Secret@123", "Female",
            new DateOnly(1995, 5, 5), "Da Nang", roleCodes);

    [Fact]
    public async Task Handle_CreatesVerifiedUser_WithNormalizedEmail_AndReputationProfile()
    {
        User? created = null;
        _ = _userRepository.AddAsync(Arg.Do<User>(user => created = user), Arg.Any<CancellationToken>());

        var result = await CreateSut().Handle(Command(new[] { "SELLER" }), CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal("new@test.local", created!.Email); // lowercased
        Assert.True(created.IsEmailConfirmed);
        Assert.Equal("HASHED", created.PasswordHash);
        Assert.Equal("new@test.local", result.Email);
        await _userRepository.Received(1).AddReputationProfileAsync(
            Arg.Any<ReputationProfile>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AssignsRequestedRoles()
    {
        User? created = null;
        _ = _userRepository.AddAsync(Arg.Do<User>(user => created = user), Arg.Any<CancellationToken>());

        var result = await CreateSut().Handle(
            Command(new[] { "SELLER", "support_staff" }), CancellationToken.None);

        Assert.Equal(new[] { "SELLER", "SUPPORT_STAFF" }, result.Roles);
        Assert.Equal(2, created!.UserRoles.Count);
    }

    [Fact]
    public async Task Handle_DefaultsToBuyer_WhenNoRolesProvided()
    {
        var result = await CreateSut().Handle(Command(roleCodes: null), CancellationToken.None);

        Assert.Equal(new[] { "BUYER" }, result.Roles);
    }

    [Fact]
    public async Task Handle_DeduplicatesRoleCodes()
    {
        User? created = null;
        _ = _userRepository.AddAsync(Arg.Do<User>(user => created = user), Arg.Any<CancellationToken>());

        var result = await CreateSut().Handle(
            Command(new[] { "BUYER", "buyer" }), CancellationToken.None);

        Assert.Equal(new[] { "BUYER" }, result.Roles);
        Assert.Single(created!.UserRoles);
    }

    [Fact]
    public async Task Handle_WritesAuditWithAssignedRoles()
    {
        UserAuditLog? audit = null;
        _ = _userRepository.AddAuditLogAsync(Arg.Do<UserAuditLog>(log => audit = log), Arg.Any<CancellationToken>());

        await CreateSut().Handle(Command(new[] { "SELLER" }), CancellationToken.None);

        Assert.NotNull(audit);
        Assert.Equal(UserAuditActions.UserCreated, audit!.Action);
        Assert.Contains("SELLER", audit.NewValue);
    }

    [Fact]
    public async Task Handle_Throws_WhenEmailAlreadyExists()
    {
        _userRepository.CheckEmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().Handle(Command(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenPhoneAlreadyExists()
    {
        _userRepository.CheckPhoneExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().Handle(Command(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenRoleCodeInvalid()
    {
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().Handle(Command(new[] { "WIZARD" }), CancellationToken.None));

        Assert.Contains("WIZARD", exception.Message);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Throws_WhenNotAuthenticated()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns((Guid?)null);
        var sut = new CreateUserCommandHandler(
            _userRepository, _roleRepository, _passwordHasher, currentUser, _unitOfWork, _publish);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.Handle(Command(), CancellationToken.None));
    }
}
