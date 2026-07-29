using System.Security.Claims;
using ECommerceAuction.UserService.Api.Contracts.Requests.PaymentMethods;
using ECommerceAuction.UserService.Application.Features.PaymentMethods.CompleteBankVerification;
using ECommerceAuction.UserService.Application.Features.PaymentMethods.StartBankVerification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/payment-methods")]
public sealed class PaymentMethodsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentMethodsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost("bank-verifications")]
    public async Task<IActionResult> StartBankVerification(
        [FromBody] StartBankVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }

        var callbackUrl = Url.ActionLink(
            nameof(CompleteBankVerification),
            values: null,
            protocol: Request.Scheme)
            ?? throw new InvalidOperationException("Could not create callback URL.");
        var result = await _sender.Send(
            new StartBankVerificationCommand(
                userId,
                request.BankCode,
                request.AccountNumber,
                request.AccountName,
                callbackUrl),
            cancellationToken);
        return Ok(new
        {
            message = "Bank verification started successfully.",
            data = result
        });
    }

    [AllowAnonymous]
    [HttpPost("bank-verifications/callback")]
    public async Task<IActionResult> CompleteBankVerification(
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        var headers = Request.Headers.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);
        var result = await _sender.Send(
            new CompleteBankVerificationCommand(headers, rawBody),
            cancellationToken);
        return Ok(new
        {
            message = "Bank verification callback processed successfully.",
            data = result
        });
    }
}
