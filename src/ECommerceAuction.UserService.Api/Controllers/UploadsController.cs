using System.Security.Claims;
using ECommerceAuction.UserService.Application.Features.Users.SetAvatar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Upload.Models;
using Nexus.Upload.Services;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Image upload endpoints backed by the shared Nexus.Upload library. Each endpoint validates the
/// file (size / MIME / dimensions per image type) and stores it via the configured provider
/// (Local for dev, DigitalOcean Spaces in Production), returning the public URL and storage key.
/// The avatar endpoint additionally persists the resulting URL onto the user's profile.
/// </summary>
[ApiController]
[Route("api/v1/uploads")]
[Authorize]
public sealed class UploadsController : ControllerBase
{
    private const string ServiceName = "user";

    private readonly UploadService _uploadService;
    private readonly ISender _sender;

    public UploadsController(UploadService uploadService, ISender sender)
    {
        _uploadService = uploadService;
        _sender = sender;
    }

    /// <summary>
    /// POST /api/v1/uploads/avatar — upload the authenticated user's avatar
    /// (max 2MB, jpeg/png/webp, 100–2000px) and save its URL onto the profile.
    /// </summary>
    [HttpPost("avatar")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var (result, error) = await PerformUploadAsync(file, "avatar", cancellationToken);
        if (error is not null)
        {
            return error;
        }

        // Persist the new avatar (URL + key) onto the user's profile.
        var response = await _sender.Send(new SetAvatarCommand(result!.Url, result.Key), cancellationToken);

        // Best-effort cleanup of the previous avatar file so old images don't orphan in storage.
        // A failure here must not fail the upload the user just made.
        if (!string.IsNullOrEmpty(response.PreviousKey))
        {
            try
            {
                await _uploadService.DeleteImageAsync(response.PreviousKey);
            }
            catch
            {
                // The old file will simply remain in storage; not worth failing the request.
            }
        }

        return Ok(new
        {
            message = "Avatar updated.",
            data = new { result.Url, result.Key, result.Size }
        });
    }

    /// <summary>
    /// POST /api/v1/uploads/identity — upload an identity document
    /// (max 5MB, jpeg/png/pdf, min 600×400). Returns the URL/key; the identity-verification
    /// flow is responsible for recording it.
    /// </summary>
    [HttpPost("identity")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadIdentity(IFormFile file, CancellationToken cancellationToken)
    {
        var (result, error) = await PerformUploadAsync(file, "identity", cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return Ok(new
        {
            message = "Upload successful.",
            data = new { result!.Url, result.Key, result.Size }
        });
    }

    /// <summary>
    /// DELETE /api/v1/uploads?key=... — delete a previously uploaded file the user owns.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "A storage key is required." });
        }

        var userId = GetUserId();
        // Keys are "{service}/{type}/{userId}/...". Only allow deleting the caller's own files.
        if (!key.Contains($"/{userId.ToLowerInvariant()}/", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        await _uploadService.DeleteImageAsync(key);
        return NoContent();
    }

    /// <summary>
    /// Validates and uploads a file. Returns the upload result on success, or an
    /// <see cref="IActionResult"/> error to return to the caller (400) on failure.
    /// </summary>
    private async Task<(UploadResult? Result, IActionResult? Error)> PerformUploadAsync(
        IFormFile file,
        string imageType,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return (null, BadRequest(new { message = "No file was uploaded." }));
        }

        var content = await ReadAllBytesAsync(file, cancellationToken);

        var options = new UploadOptions
        {
            Service = ServiceName,
            ImageType = imageType,
            UserId = GetUserId(),
            FileName = file.FileName,
            MimeType = file.ContentType,
            Size = file.Length
        };

        try
        {
            var result = await _uploadService.UploadImageAsync(content, options);
            return (result, null);
        }
        catch (InvalidOperationException exception)
        {
            // Thrown by the library when validation fails (bad MIME/size/dimensions).
            return (null, BadRequest(new { message = exception.Message }));
        }
    }

    private static async Task<byte[]> ReadAllBytesAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
    }
}
