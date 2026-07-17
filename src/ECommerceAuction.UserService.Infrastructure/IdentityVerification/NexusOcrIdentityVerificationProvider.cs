using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Services.IdentityMatching;
using Microsoft.Extensions.Logging;
using Nexus.Ocr;
using Nexus.Ocr.Models;
using Nexus.Upload.src.Core;

namespace ECommerceAuction.UserService.Infrastructure.IdentityVerification;

/// <summary>
/// Reads the card with Nexus.Ocr (QR first, OCR fallback), pulling the image straight out of storage
/// by key rather than fetching its public URL over HTTP.
/// </summary>
public sealed class NexusOcrIdentityVerificationProvider : IIdentityVerificationProvider
{
    private readonly IStorageProvider _storageProvider;
    private readonly IdentityReader _identityReader;
    private readonly ILogger<NexusOcrIdentityVerificationProvider> _logger;

    public NexusOcrIdentityVerificationProvider(
        IStorageProvider storageProvider,
        IdentityReader identityReader,
        ILogger<NexusOcrIdentityVerificationProvider> logger)
    {
        _storageProvider = storageProvider;
        _identityReader = identityReader;
        _logger = logger;
    }

    public async Task<IdentityExtractionResult> ExtractAsync(
        string frontImageKey,
        string backImageKey,
        CancellationToken cancellationToken)
    {
        var image = await _storageProvider.DownloadAsync(frontImageKey);
        if (image is null || image.Length == 0)
        {
            _logger.LogWarning(
                "Identity verification could not read the stored front image {Key}.",
                frontImageKey);

            return IdentityExtractionResult.Failed(
                "The uploaded identity card image could not be read. Please upload it again.");
        }

        IdentityInfo info;
        try
        {
            // IdentityReader is synchronous and CPU-bound, and the OCR engine is a shared singleton
            // that serialises on an internal lock — keep it off the request thread.
            info = await Task.Run(() => _identityReader.Read(image), cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Reading the identity card image {Key} failed.", frontImageKey);

            return IdentityExtractionResult.Failed(
                "The identity card image could not be processed. Please upload a clearer photo.");
        }

        _logger.LogInformation(
            "Identity card read via {Source} with confidence {Confidence}.",
            info.Source,
            info.Confidence);

        var extraction = new IdentityExtraction(
            info.FullName,
            info.Gender,
            info.DateOfBirth,
            info.IdentityNumber,
            (decimal)info.Confidence);

        return new IdentityExtractionResult(
            Success: true,
            Extraction: extraction,
            ExtractedIssueDate: info.IssueDate,
            ExtractedPermanentAddress: info.Address,
            FailureReason: null);
    }
}