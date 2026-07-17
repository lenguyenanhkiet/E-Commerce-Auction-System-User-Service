using ECommerceAuction.UserService.Application.Services.IdentityMatching;

namespace ECommerceAuction.UserService.Application.UnitTests.Identities;

public sealed class IdentityMatcherTests
{
    private const decimal MinimumConfidence = 0.80m;

    private static IdentityDeclaration Declared(
        string fullName = "NGUYỄN VĂN A",
        string gender = "Nam",
        string identityNumber = "079201001234") =>
        new(fullName, gender, new DateOnly(1990, 1, 1), identityNumber);

    private static IdentityExtraction Extracted(
        string? fullName = "NGUYỄN VĂN A",
        string? gender = "Nam",
        DateOnly? dateOfBirth = null,
        string? identityNumber = "079201001234",
        decimal confidence = 1.0m) =>
        new(fullName, gender, dateOfBirth ?? new DateOnly(1990, 1, 1), identityNumber, confidence);

    [Fact]
    public void Match_Succeeds_WhenEveryFieldAgrees()
    {
        var outcome = IdentityMatcher.Match(Declared(), Extracted(), MinimumConfidence);

        Assert.True(outcome.IsMatch);
        Assert.Null(outcome.FailureReason);
    }

    [Fact]
    public void Match_Fails_WhenIdentityNumberDiffers()
    {
        // The whole point of the feature: the number the user typed must be the one on the card.
        var outcome = IdentityMatcher.Match(
            Declared(identityNumber: "079201001234"),
            Extracted(identityNumber: "999999999999"),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
        Assert.NotNull(outcome.FailureReason);
    }

    [Fact]
    public void Match_Succeeds_WhenNamesDifferOnlyByDiacritics()
    {
        // OCR routinely drops accents; that must not reject an honest submission.
        var outcome = IdentityMatcher.Match(
            Declared(fullName: "NGUYỄN VĂN A"),
            Extracted(fullName: "NGUYEN VAN A"),
            MinimumConfidence);

        Assert.True(outcome.IsMatch);
    }

    [Fact]
    public void Match_Fails_WhenNamesAreDifferentPeople()
    {
        var outcome = IdentityMatcher.Match(
            Declared(fullName: "NGUYỄN VĂN A"),
            Extracted(fullName: "TRẦN THỊ B"),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
        Assert.NotNull(outcome.FailureReason);
    }

    [Fact]
    public void Match_Fails_WhenDateOfBirthDiffers()
    {
        var outcome = IdentityMatcher.Match(
            Declared(),
            Extracted(dateOfBirth: new DateOnly(1991, 2, 3)),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
    }

    [Fact]
    public void Match_Fails_WhenGenderDiffers()
    {
        var outcome = IdentityMatcher.Match(
            Declared(gender: "Nam"),
            Extracted(gender: "Nữ"),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
    }

    [Fact]
    public void Match_Fails_WhenConfidenceIsBelowThreshold()
    {
        var outcome = IdentityMatcher.Match(
            Declared(),
            Extracted(confidence: 0.42m),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
        Assert.NotNull(outcome.FailureReason);
    }

    [Fact]
    public void Match_Fails_WhenIdentityNumberCouldNotBeRead()
    {
        // A blurry card yields nulls — that is "unknown", never "matches".
        var outcome = IdentityMatcher.Match(
            Declared(),
            Extracted(identityNumber: null),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
        Assert.NotNull(outcome.FailureReason);
    }

    [Fact]
    public void Match_Fails_WhenNameCouldNotBeRead()
    {
        var outcome = IdentityMatcher.Match(
            Declared(),
            Extracted(fullName: null),
            MinimumConfidence);

        Assert.False(outcome.IsMatch);
    }

    [Fact]
    public void Match_Succeeds_WhenDateOfBirthCouldNotBeRead()
    {
        // Only the number and the name are load-bearing; a missing DOB must not reject on its own.
        // Built inline because the Extracted helper substitutes a default for a null date.
        var extraction = new IdentityExtraction(
            FullName: "NGUYỄN VĂN A",
            Gender: "Nam",
            DateOfBirth: null,
            IdentityNumber: "079201001234",
            Confidence: 1.0m);

        var outcome = IdentityMatcher.Match(Declared(), extraction, MinimumConfidence);

        Assert.True(outcome.IsMatch);
    }
}
