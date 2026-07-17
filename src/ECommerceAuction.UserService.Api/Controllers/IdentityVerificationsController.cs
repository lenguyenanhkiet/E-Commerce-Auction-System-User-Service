using ECommerceAuction.UserService.Api.Controllers.Contracts.Requests;
using ECommerceAuction.UserService.Application.Features.Identities.GetMyIdentityVerification;
using ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Upload.src.Core;
using System.Security.Claims;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/identity-verifications")]
[Authorize]
public class IdentityVerificationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IStorageProvider _storageProvider;

    public IdentityVerificationsController(ISender sender, IStorageProvider storageProvider)
    {
        _sender = sender;
        _storageProvider = storageProvider;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitIdentityVerificationRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitIdentityVerificationCommand(
            UserId: GetCurrentUserId(),
            FullName: request.FullName,
            Gender: request.Gender,
            DateOfBirth: request.DateOfBirth,
            IdentityNumber: request.IdentityNumber,
            IssueDate: request.IssueDate,
            ExpiryDate: request.ExpiryDate,
            IssuePlace: request.IssuePlace,
            PermanentAddress: request.PermanentAddress,
            FrontImageKey: request.FrontImageKey,
            BackImageKey: request.BackImageKey);

        var response = await _sender.Send(command, cancellationToken);

        // Best-effort cleanup of the images this submission replaced, so rejected attempts don't
        // orphan files forever. A failure here must not fail the submission the user just made.
        foreach (var previousKey in response.PreviousImageKeys)
        {
            try
            {
                await _storageProvider.DeleteAsync(previousKey);
            }
            catch
            {
                // The old file simply stays in storage; not worth failing the request.
            }
        }

        return Ok(new
        {
            message = response.status == "Verified"
            ? "Identity verified successfully."
            : "Identity verification failed.",
            data = new
            {
                response.Id,
                response.status,
                response.ConfidenceScore,
                response.RejectionReason
            }
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyIdentityVerificationQuery(GetCurrentUserId()), cancellationToken);

        if (result is null)
        {
            return Ok(new { message = "No identity verification found.", data = (object?)null });
        }

        // The record stores storage keys; callers need something they can actually load. Resolving
        // here rather than storing the URL keeps old rows working when the storage host changes.
        return Ok(new
        {
            message = "Retrieved identity verification successfully.",
            data = new
            {
                result.Id,
                result.FullName,
                result.Gender,
                result.DateOfBirth,
                result.IdentityNumber,
                result.IssueDate,
                result.ExpiryDate,
                result.IssuePlace,
                result.PermanentAddress,
                IdentityFrontImageUrl = _storageProvider.GetUrl(result.IdentityFrontImageKey),
                IdentityBackImageUrl = _storageProvider.GetUrl(result.IdentityBackImageKey),
                result.Status,
                result.RejectionReason,
                result.SubmittedAt,
                result.VerifiedAt
            }
        });
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
    }
}