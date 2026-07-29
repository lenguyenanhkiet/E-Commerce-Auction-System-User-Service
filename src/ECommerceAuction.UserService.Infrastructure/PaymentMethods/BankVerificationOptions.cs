namespace ECommerceAuction.UserService.Infrastructure.PaymentMethods;

public sealed class BankVerificationOptions
{
    public const string SectionName = "BankVerification";

    public string ProviderCode { get; init; } = "SANDBOX";
    public string CallbackSecret { get; init; } = "development-only-bank-secret";
    public int ExpirationMinutes { get; init; } = 15;
}
