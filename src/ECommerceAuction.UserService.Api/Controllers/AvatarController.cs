using ECommerceAuction.UserService.Application.Features.Users.Avatar.RemoveAvatar;
using ECommerceAuction.UserService.Application.Features.Users.Avatar.SetAvatar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Upload.Models;
using Nexus.Upload.Services;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users/me/avatar")]
public sealed class AvatarController : ControllerBase
{
    private const string ServiceName = "user";

    private readonly UploadService _uploadService;
    private readonly ISender _sender;
    private readonly ILogger<AvatarController> _logger;

    public AvatarController(
        UploadService uploadService,
        ISender sender,
        ILogger<AvatarController> logger)
    {
        _uploadService = uploadService;
        _sender = sender;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "No file was uploaded."
            });
        }

        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException(
                "Authenticated user identifier is missing.");

        byte[] content;

        await using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream, cancellationToken);
            content = stream.ToArray();
        }

        UploadResult uploaded;

        try
        {
            uploaded = await _uploadService.UploadImageAsync(
                content,
                new UploadOptions
                {
                    Service = ServiceName,
                    ImageType = "avatar",
                    UserId = userId,
                    FileName = file.FileName,
                    MimeType = file.ContentType,
                    Size = file.Length
                });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }

        SetAvatarResponse response;

        try
        {
            response = await _sender.Send(
                new SetAvatarCommand(
                    uploaded.Url,
                    uploaded.Key),
                cancellationToken);
        }
        catch
        {
            // Upload đã thành công nhưng DB thất bại:
            // xóa file mới để không tạo file rác.
            await TryDeleteAsync(
                uploaded.Key,
                userId,
                "new avatar rollback");

            throw;
        }

        if (!string.IsNullOrWhiteSpace(response.PreviousKey))
        {
            await TryDeleteAsync(
                response.PreviousKey,
                userId,
                "previous avatar cleanup");
        }

        return Ok(new
        {
            message = "Avatar updated successfully.",
            data = new
            {
                avatarUrl = response.AvatarUrl,
                uploaded.Size
            }
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Remove(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException(
                "Authenticated user identifier is missing.");

        var response = await _sender.Send(
            new RemoveAvatarCommand(),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(response.PreviousKey))
        {
            await TryDeleteAsync(
                response.PreviousKey,
                userId,
                "avatar removal");
        }

        return NoContent();
    }

    private async Task TryDeleteAsync(
        string key,
        string userId,
        string operation)
    {
        try
        {
            await _uploadService.DeleteImageAsync(key);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not delete avatar file during {Operation}. UserId: {UserId}, Key: {StorageKey}",
                operation,
                userId,
                key);
        }
    }
}
