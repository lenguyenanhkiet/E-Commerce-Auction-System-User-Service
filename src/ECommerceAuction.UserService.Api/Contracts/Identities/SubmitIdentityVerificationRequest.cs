namespace ECommerceAuction.UserService.Api.Contracts.Identities;

public sealed record SubmitIdentityVerificationRequest
(
    string FullName,
    string Gender,
    DateOnly DateOfBirth,
    string IdentityNumber,
    DateOnly IssueDate,
    DateOnly ExpiryDate,
    string IssuePlace,
    string PermanentAddress,
    string FrontImageKey,
    string BackImageKey
);