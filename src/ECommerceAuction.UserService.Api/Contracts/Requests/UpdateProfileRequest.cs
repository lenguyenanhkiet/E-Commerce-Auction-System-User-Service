namespace ECommerceAuction.UserService.Api.Contracts.Requests;

public sealed record UpdateProfileRequest(
    string PhoneNumber, //New phone number


    string? NewEmail); //New email (can be null if you do not want to change)