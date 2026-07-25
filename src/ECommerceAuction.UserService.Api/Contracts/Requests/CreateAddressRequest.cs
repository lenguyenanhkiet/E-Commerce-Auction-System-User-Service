namespace ECommerceAuction.UserService.Api.Contracts.Requests;

public sealed record CreateAddressRequest
    (
        string RecipientName,
        string RecipientPhone,
        string Province,
        string Ward,
        string Street,
        string Type,
        bool IsDefault
    );