using ECommerceAuction.UserService.Application.Services.IdentityMatching;

namespace ECommerceAuction.UserService.Application.UnitTests.Identities;

public sealed class TextNormalizerTests
{
    [Fact]
    public void NormalizeName_StripsVietnameseDiacritics()
    {
        Assert.Equal("NGUYEN VAN A", TextNormalizer.NormalizeName("NGUYỄN VĂN A"));
    }

    [Fact]
    public void NormalizeName_FoldsDAndDStroke()
    {
        // đ/Đ have no canonical decomposition, so they need their own mapping.
        Assert.Equal("DINH DUC DUY", TextNormalizer.NormalizeName("Đinh Đức Duy"));
    }

    [Fact]
    public void NormalizeName_UppercasesAndCollapsesWhitespace()
    {
        Assert.Equal("TRAN THI B", TextNormalizer.NormalizeName("  Trần   Thị\tB  "));
    }

    [Fact]
    public void NormalizeName_ReturnsEmpty_WhenNullOrBlank()
    {
        Assert.Equal(string.Empty, TextNormalizer.NormalizeName(null));
        Assert.Equal(string.Empty, TextNormalizer.NormalizeName("   "));
    }

    [Theory]
    [InlineData("Nam", "NAM")]
    [InlineData("nữ", "NU")]
    [InlineData(" Nữ ", "NU")]
    public void NormalizeGender_FoldsCaseAndDiacritics(string input, string expected)
    {
        Assert.Equal(expected, TextNormalizer.NormalizeGender(input));
    }

    [Fact]
    public void NormalizeIdentityNumber_KeepsOnlyDigits()
    {
        // OCR and users alike sprinkle spaces and dots through the 12 digits.
        Assert.Equal("079201001234", TextNormalizer.NormalizeIdentityNumber(" 079 201.001-234 "));
    }
}
