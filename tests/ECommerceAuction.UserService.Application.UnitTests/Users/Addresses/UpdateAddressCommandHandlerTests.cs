using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.UpdateAddress;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Users.Addresses;

public sealed class UpdateAddressCommandHandlerTests
{
    [Fact]
    public async Task Handle_MapsStreetProvinceAndWardToTheirMatchingResponseFields()
    {
        var userId = Guid.NewGuid();
        var address = new Address(
            userId,
            "Old Recipient",
            "0900000000",
            "Old Province",
            "Old Ward",
            "Old Street",
            "Home");
        var repository = Substitute.For<IAddressRepository>();
        repository.GetByIdAsync(address.Id, Arg.Any<CancellationToken>())
            .Returns(address);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(userId);
        var handler = new UpdateAddressCommandHandler(
            repository,
            Substitute.For<IUnitOfWork>(),
            currentUser,
            Substitute.For<ILogger<UpdateAddressCommandHandler>>());
        var command = new UpdateAddressCommand(
            address.Id,
            "New Recipient",
            "0911111111",
            "New Province",
            "New Ward",
            "New Street",
            "Work",
            false);

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("New Street", response.Street);
        Assert.Equal("New Province", response.Province);
        Assert.Equal("New Ward", response.Ward);
    }
}
