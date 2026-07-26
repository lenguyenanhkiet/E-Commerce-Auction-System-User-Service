namespace ECommerceAuction.UserService.Api.Contracts.Addresses;

public sealed record UpdateAddressRequest(
        string RecipientName,
        string RecipientPhone,
        string Province,
        string Ward,
        string Street,
        string Type,
        bool IsDefault
    );