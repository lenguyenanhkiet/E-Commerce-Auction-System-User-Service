using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.Users.GetUserById;
using ECommerceAuction.UserService.Domain.Entities.IdentityVerification;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.Users;

public sealed class GetUserByIdQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    private readonly IIdentityVerificationRepository _identityRepository =
        Substitute.For<IIdentityVerificationRepository>();

    private GetUserByIdQueryHandler CreateSut() => new(_userRepository, _identityRepository);

    private static User CreateUser() =>
        User.CreateByAdmin("user@test.local", "HASH", "Test User", "0900000000", "Male",
            new DateOnly(1990, 1, 1));

    [Fact]
    public async Task Handle_ReturnsMappedDetail_WhenUserExists()
    {
        var user = CreateUser();
        var identity = new IdentityVerification(
            userId: user.Id,
            fullName: "Le Nguyen Anh Kiet",
            gender: "Nam",
            dateOfBirth: new DateOnly(1990, 1, 1),
            identityNumber: "079123456789",
            issueDate: new DateOnly(2020, 1, 1),
            expiryDate: new DateOnly(2040, 1, 1),
            issuePlace: "Cục Cảnh sát QLHC về TTXH",
            permanentAddress: "TP.HCM",
            identityFrontImageKey: "front.jpg",
            identityBackImageKey: "back.jpg"
        );

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _identityRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(identity);
        _userRepository.GetUserRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "BUYER", "SELLER" });

        var result = await CreateSut().Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user@test.local", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("079123456789", result.IdentityNumber);
        Assert.True(result.IsEmailConfirmed);
        Assert.Equal(new[] { "BUYER", "SELLER" }, result.Roles);
    }

    [Fact]
    public async Task Handle_ReturnsNullIdentityNumber_WhenNoIdentityVerification()
    {
        var user = CreateUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _identityRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns((IdentityVerification?)null);
        _userRepository.GetUserRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        var result = await CreateSut().Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.Null(result.IdentityNumber);
        Assert.Empty(result.Roles);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotFound()
    {
        var missingId = Guid.NewGuid();
        _userRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().Handle(new GetUserByIdQuery(missingId), CancellationToken.None));
    }
}