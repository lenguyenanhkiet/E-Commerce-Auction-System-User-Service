namespace ECommerceAuction.UserService.Application.Common.Exceptions;

/// <summary>
/// Base type for expected application errors that must not become HTTP 500 responses.
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(string message)
        : base(message)
    {
    }

    protected ApplicationExceptionBase(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class NotFoundException : ApplicationExceptionBase
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class ConflictException : ApplicationExceptionBase
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BusinessRuleException : ApplicationExceptionBase
{
    public BusinessRuleException(string message) : base(message) { }
}
