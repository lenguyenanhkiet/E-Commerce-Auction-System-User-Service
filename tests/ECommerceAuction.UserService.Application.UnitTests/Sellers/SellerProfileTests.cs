using ECommerceAuction.UserService.Domain.Sellers;

namespace ECommerceAuction.UserService.Application.UnitTests.Sellers;

public sealed class SellerProfileTests
{
    [Theory]
    [InlineData(null, "TAX-001", "license.jpg")]
    [InlineData("Business", null, "license.jpg")]
    [InlineData("Business", "TAX-001", null)]
    public void Constructor_RejectsMissingRequiredBusinessDetails(
        string? businessName,
        string? taxCode,
        string? businessLicenseUrl)
    {
        Assert.ThrowsAny<ArgumentException>(() => new SellerProfile(
            Guid.NewGuid(),
            SellerTypes.Business,
            businessName!,
            "0123456789",
            taxCode!,
            businessLicenseUrl!,
            "Address",
            "0123456789",
            "Bank",
            "Account Holder",
            DateTimeOffset.UtcNow));
    }
}
