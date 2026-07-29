using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers.Policies;

public sealed record SellerRegistrationEligibilityResult
{
    public bool IsEligible { get; init; }
    public IReadOnlyCollection<string> MissingRequirements
    { get; init; } = Array.Empty<string>();

    private SellerRegistrationEligibilityResult() { }
    public static SellerRegistrationEligibilityResult Eligible()
    {
        return new SellerRegistrationEligibilityResult
        {
            IsEligible = true,
            MissingRequirements = Array.Empty<string>()
        };
    }

    public static SellerRegistrationEligibilityResult NotEligible(
        IEnumerable<string> missingRequirements)
    {
        ArgumentNullException.ThrowIfNull(missingRequirements);

        var requirements = missingRequirements
            .Where(SellerRegistrationRequirements.IsValid)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new SellerRegistrationEligibilityResult
        {
            IsEligible = false,
            MissingRequirements = requirements
        };
    }
}