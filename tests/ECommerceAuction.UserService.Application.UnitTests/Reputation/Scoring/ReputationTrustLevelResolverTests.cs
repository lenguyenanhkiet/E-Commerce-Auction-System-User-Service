using ECommerceAuction.UserService.Domain.Reputation.Common;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Scoring;

public sealed class ReputationTrustLevelResolverTests
{
    [Theory]
    [InlineData(int.MinValue, "RESTRICTED")]
    [InlineData(-1, "RESTRICTED")]
    [InlineData(0, "BASIC")]
    [InlineData(49, "BASIC")]
    [InlineData(50, "TRUSTED")]
    [InlineData(199, "TRUSTED")]
    [InlineData(200, "RELIABLE")]
    [InlineData(499, "RELIABLE")]
    [InlineData(500, "PREMIUM")]
    [InlineData(999, "PREMIUM")]
    [InlineData(1_000, "ELITE")]
    [InlineData(int.MaxValue, "ELITE")]
    public void Resolve_UsesApprovedBoundaries(
        int confirmedScore,
        string expected)
    {
        var actual =
            ReputationTrustLevelResolver.Resolve(confirmedScore);

        Assert.Equal(expected, actual);
    }
}
