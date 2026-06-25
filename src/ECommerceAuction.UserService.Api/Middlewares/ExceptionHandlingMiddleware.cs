using FluentValidation;
using System.Text.Json;

namespace ECommerceAuction.UserService.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            // FluentValidation validation errors.
            ValidationException =>
                StatusCodes.Status400BadRequest,

            // Invalid business data, duplicate email, invalid page, etc.
            InvalidOperationException =>
                StatusCodes.Status400BadRequest,

            // JWT is missing or does not contain a valid user id.
            UnauthorizedAccessException =>
                StatusCodes.Status401Unauthorized,

            // The requested user does not exist.
            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            // Unexpected system error.
            _ =>
                StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "An unexpected server error occurred.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode}.",
                statusCode);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var detail = exception switch
        {
            ValidationException validationException =>
                string.Join(
                    "; ",
                    validationException.Errors
                        .Select(error => error.ErrorMessage)),

            _ => exception.Message
        };

        var response = new
        {
            title = GetTitle(statusCode),
            status = statusCode,
            detail
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "Bad Request",

            StatusCodes.Status401Unauthorized =>
                "Unauthorized",

            StatusCodes.Status404NotFound =>
                "Not Found",

            _ =>
                "Internal Server Error"
        };
    }
}