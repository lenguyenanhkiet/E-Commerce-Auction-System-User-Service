using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification;
using ECommerceAuction.UserService.Application.Services.IdentityMatching;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;

namespace ECommerceAuction.UserService.Application.UnitTests.Identities;

public sealed class SubmitIdentityVerificationCommandHandlerTests
{
    private static readonly Guid CallerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VictimId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IIdentityVerificationRepository _verificationRepository =
        Substitute.For<IIdentityVerificationRepository>();

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly IIdentityVerificationProvider _provider =
        Substitute.For<IIdentityVerificationProvider>();

    private readonly IReputationAwardService _awardService =
        Substitute.For<IReputationAwardService>();

    private SubmitIdentityVerificationCommandHandler CreateSut()
    {
        var user = new User("caller@example.com", "hash", "NGUYỄN VĂN A", "0900000000");
        _userRepository.GetByIdAsync(CallerId, Arg.Any<CancellationToken>()).Returns(user);

        var completeIdentity =
            new ECommerceAuction.UserService.Application.Features.Identities.VerifyIdentity.CompleteIdentityVerificationService(
                Substitute.For<IBuyerVerificationRepository>(),
                _awardService);

        var options = Options.Create(new IdentityMatchingOptions { MinimumConfidence = 0.80m });
        return new SubmitIdentityVerificationCommandHandler(
            _verificationRepository, _userRepository, _unitOfWork, _provider, options, completeIdentity);
    }

    private static SubmitIdentityVerificationCommand Command(
        string frontKey = $"user/identity/11111111-1111-1111-1111-111111111111/front.jpg",
        string backKey = $"user/identity/11111111-1111-1111-1111-111111111111/back.jpg") =>
        new(
            UserId: CallerId,
            FullName: "NGUYỄN VĂN A",
            Gender: "Nam",
            DateOfBirth: new DateOnly(1990, 1, 1),
            IdentityNumber: "079201001234",
            IssueDate: new DateOnly(2021, 1, 1),
            ExpiryDate: new DateOnly(2039, 1, 1),
            IssuePlace: "Cục Cảnh sát QLHC về TTXH",
            PermanentAddress: "Quận 1, TP. Hồ Chí Minh",
            FrontImageKey: frontKey,
            BackImageKey: backKey);

    private void ProviderReads(
        string identityNumber = "079201001234",
        string fullName = "NGUYỄN VĂN A",
        decimal confidence = 1.0m)
    {
        _provider.ExtractAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new IdentityExtractionResult(
                Success: true,
                Extraction: new IdentityExtraction(
                    fullName, "Nam", new DateOnly(1990, 1, 1), identityNumber, confidence),
                ExtractedIssueDate: null,
                ExtractedPermanentAddress: null,
                FailureReason: null));
    }

    [Fact]
    public async Task Handle_Throws_WhenFrontImageKeyBelongsToAnotherUser()
    {
        // Keys are "user/identity/{userId}/...", so one user could submit a victim's uploaded card,
        // let OCR read the victim's details, declare those details, and be verified as the victim.
        ProviderReads();
        var command = Command(frontKey: $"user/identity/{VictimId}/front.jpg");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateSut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenBackImageKeyBelongsToAnotherUser()
    {
        ProviderReads();
        var command = Command(backKey: $"user/identity/{VictimId}/back.jpg");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateSut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DoesNotReadTheCard_WhenKeyOwnershipFails()
    {
        // The ownership check must come first: no storage read, no OCR work for a rejected caller.
        ProviderReads();
        var command = Command(frontKey: $"user/identity/{VictimId}/front.jpg");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateSut().Handle(command, CancellationToken.None));

        await _provider.DidNotReceive().ExtractAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Rejects_WhenCardNumberDiffersFromDeclaration()
    {
        // The reason this feature exists: the mock used to pass any 12-digit number.
        ProviderReads(identityNumber: "999999999999");

        var response = await CreateSut().Handle(Command(), CancellationToken.None);

        Assert.Equal("Rejected", response.status);
        Assert.NotNull(response.RejectionReason);
    }

    [Fact]
    public async Task Handle_Verifies_WhenCardAgreesWithDeclaration()
    {
        ProviderReads();

        var response = await CreateSut().Handle(Command(), CancellationToken.None);

        Assert.Equal("Verified", response.status);
        Assert.Null(response.RejectionReason);
    }

    [Fact]
    public async Task Handle_Rejects_WhenCardCouldNotBeRead()
    {
        _provider.ExtractAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(IdentityExtractionResult.Failed("The image could not be read."));

        var response = await CreateSut().Handle(Command(), CancellationToken.None);

        Assert.Equal("Rejected", response.status);
        Assert.Equal("The image could not be read.", response.RejectionReason);
    }
}
