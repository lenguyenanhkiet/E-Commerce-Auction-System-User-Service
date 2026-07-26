namespace ECommerceAuction.UserService.Api.Contracts.Requests.PaymentMethods;

public sealed record StartBankVerificationRequest
{
    public required string BankCode { get; init; }
    public required string AccountNumber { get; init; }
    public required string AccountName { get; init; }
}
