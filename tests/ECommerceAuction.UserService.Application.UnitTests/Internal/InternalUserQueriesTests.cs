using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Sellers;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Repositories;
using ECommerceAuction.UserService.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Application.UnitTests.Internal;

public sealed class InternalUserQueriesTests
{
    [Fact]
    public async Task BuyerEligibility_Verifies_owned_active_address_snapshot()
    {
        await using var db = CreateDbContext();
        var buyer = CreateActivePhoneVerifiedUser();
        var address = new Address(
            buyer.Id,
            "Nguyen Van A",
            "090 123 4567",
            "Ho Chi Minh",
            "Ward 1",
            "12 Nguyen Hue",
            "Home",
            isDefault: true);

        db.Users.Add(buyer);
        db.Addresses.Add(address);
        await db.SaveChangesAsync();

        var result = await new InternalUserQueries(db).GetBuyerCheckoutEligibilityAsync(
            buyer.Id,
            new BuyerAddressCandidateSnapshot(
                "  nguyen   van a ",
                "090-123-4567",
                "12  nguyen hue",
                null,
                "ward 1",
                "District 1",
                "ho chi minh",
                "VN",
                null),
            CancellationToken.None);

        Assert.True(result.AddressVerified);
        Assert.True(result.CanPurchase);
        Assert.DoesNotContain("ADDRESS_MISMATCH", result.Issues);
    }

    [Fact]
    public async Task BuyerEligibility_Rejects_address_owned_by_another_user()
    {
        await using var db = CreateDbContext();
        var buyer = CreateActivePhoneVerifiedUser();
        var other = CreateActivePhoneVerifiedUser();
        db.Users.AddRange(buyer, other);
        db.Addresses.Add(new Address(
            other.Id,
            "Nguyen Van A",
            "0901234567",
            "Ho Chi Minh",
            "Ward 1",
            "12 Nguyen Hue",
            "Home",
            isDefault: true));
        await db.SaveChangesAsync();

        var result = await new InternalUserQueries(db).GetBuyerCheckoutEligibilityAsync(
            buyer.Id,
            MatchingAddress(),
            CancellationToken.None);

        Assert.False(result.AddressVerified);
        Assert.False(result.CanPurchase);
        Assert.Contains("BUYER_ADDRESS_NOT_VERIFIED", result.Issues);
    }

    [Fact]
    public async Task BuyerEligibility_Rejects_deleted_or_missing_address_candidate()
    {
        await using var db = CreateDbContext();
        var buyer = CreateActivePhoneVerifiedUser();
        var deletedAddress = new Address(
            buyer.Id,
            "Nguyen Van A",
            "0901234567",
            "Ho Chi Minh",
            "Ward 1",
            "12 Nguyen Hue",
            "Home",
            isDefault: true);
        deletedAddress.Delete();

        db.Users.Add(buyer);
        db.Addresses.Add(deletedAddress);
        await db.SaveChangesAsync();

        var queries = new InternalUserQueries(db);
        var deletedResult = await queries.GetBuyerCheckoutEligibilityAsync(
            buyer.Id,
            MatchingAddress(),
            CancellationToken.None);
        var missingCandidateResult = await queries.GetBuyerCheckoutEligibilityAsync(
            buyer.Id,
            null,
            CancellationToken.None);

        Assert.False(deletedResult.AddressVerified);
        Assert.Contains("BUYER_ADDRESS_NOT_VERIFIED", deletedResult.Issues);
        Assert.False(missingCandidateResult.AddressVerified);
        Assert.Contains("BUYER_ADDRESS_NOT_VERIFIED", missingCandidateResult.Issues);
    }

    [Fact]
    public async Task BuyerEligibility_Inactive_or_missing_user_is_not_eligible()
    {
        await using var db = CreateDbContext();
        var buyer = CreateActivePhoneVerifiedUser();
        buyer.AdminSoftDeleteUser();
        db.Users.Add(buyer);
        await db.SaveChangesAsync();

        var queries = new InternalUserQueries(db);
        var inactive = await queries.GetBuyerCheckoutEligibilityAsync(
            buyer.Id,
            MatchingAddress(),
            CancellationToken.None);
        var missing = await queries.GetBuyerCheckoutEligibilityAsync(
            Guid.NewGuid(),
            MatchingAddress(),
            CancellationToken.None);

        Assert.False(inactive.CanPurchase);
        Assert.False(missing.CanPurchase);
        Assert.Contains("USER_NOT_FOUND", missing.Issues);
    }

    [Fact]
    public async Task SellerCommerceProfile_Requires_approved_profile_and_active_seller_role()
    {
        await using var db = CreateDbContext();
        var seller = CreateActivePhoneVerifiedUser();
        seller.AssignRole(UserRole.Assign(seller.Id, RbacSeedData.SellerRoleId));
        var profile = CreateSellerProfile(seller.Id);
        profile.Approve(Guid.NewGuid());

        db.Users.Add(seller);
        db.SellerProfiles.Add(profile);
        await db.SaveChangesAsync();

        var result = await new InternalUserQueries(db).GetSellerCommerceProfilesAsync(
            [seller.Id],
            CancellationToken.None);

        var item = Assert.Single(result);
        Assert.True(item.CanSell);
        Assert.Empty(item.Issues);
    }

    [Fact]
    public async Task SellerCommerceProfile_Rejects_revoked_or_missing_seller_role()
    {
        await using var db = CreateDbContext();
        var revokedSeller = CreateActivePhoneVerifiedUser();
        var revokedRole = UserRole.Assign(revokedSeller.Id, RbacSeedData.SellerRoleId);
        revokedRole.Revoke();
        revokedSeller.AssignRole(revokedRole);
        var profile = CreateSellerProfile(revokedSeller.Id);
        profile.Approve(Guid.NewGuid());

        db.Users.Add(revokedSeller);
        db.SellerProfiles.Add(profile);
        await db.SaveChangesAsync();

        var result = await new InternalUserQueries(db).GetSellerCommerceProfilesAsync(
            [revokedSeller.Id, Guid.NewGuid(), revokedSeller.Id],
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, item =>
            item.RequestedSellerUserId == revokedSeller.Id &&
            !item.CanSell &&
            item.Issues.Contains("SELLER_ROLE_INACTIVE"));
        Assert.Contains(result, item =>
            !item.Found &&
            item.Issues.Contains("SELLER_NOT_FOUND"));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateActivePhoneVerifiedUser()
    {
        var user = new User(
            Guid.NewGuid(),
            $"{Guid.NewGuid():N}@example.test",
            "hash",
            "Test User",
            "0901234567");
        user.ConfirmPhoneChange();
        return user;
    }

    private static SellerProfile CreateSellerProfile(Guid userId) =>
        new(
            userId,
            "Individual",
            "Test Shop",
            "TAX-001",
            "https://example.test/license.png",
            "12 Nguyen Hue",
            "123456789",
            "Test Bank",
            "Test Owner");

    private static BuyerAddressCandidateSnapshot MatchingAddress() =>
        new(
            "Nguyen Van A",
            "0901234567",
            "12 Nguyen Hue",
            null,
            "Ward 1",
            "District 1",
            "Ho Chi Minh",
            "VN",
            null);
}
