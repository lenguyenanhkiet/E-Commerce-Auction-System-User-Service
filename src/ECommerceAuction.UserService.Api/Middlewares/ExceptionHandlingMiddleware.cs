using FluentValidation;
using System.Text.Json;
using ECommerceAuction.UserService.Application.Common.Exceptions;

namespace ECommerceAuction.UserService.Api.Middlewares;

/// <summary>
/// Converts expected validation and business exceptions to stable HTTP responses.
/// </summary>
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
            var isExpectedException = exception is ApplicationExceptionBase or
                ValidationException or
                UnauthorizedAccessException;

            if (isExpectedException)
            {
                _logger.LogWarning(exception, "Request failed with an expected application error");
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception occurred");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                BusinessRuleException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var validationErrors = exception is ValidationException validationException
                ? validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray())
                : null;

            var detail = context.Response.StatusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected server error occurred."
                : exception.Message;

            var response = new
            {
                title = GetTitle(context.Response.StatusCode),
                status = context.Response.StatusCode,
                detail,
                errors = validationErrors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }


    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Server Error"
        };
    }
}
