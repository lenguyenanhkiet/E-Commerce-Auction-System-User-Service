namespace ECommerceAuction.UserService.Api.Controllers.Contracts.Requests;

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