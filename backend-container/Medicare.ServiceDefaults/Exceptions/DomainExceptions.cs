namespace Medicare.ServiceDefaults.Exceptions;

public abstract class DomainException : Exception
{
    public int StatusCode { get; }

    protected DomainException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    protected DomainException(string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404) { }
    public NotFoundException(string entityName, object id) : base($"{entityName} with id '{id}' was not found.", 404) { }
}

public class BadRequestException : DomainException
{
    public BadRequestException(string message) : base(message, 400) { }
}

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized access.") : base(message, 401) { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Access forbidden.") : base(message, 403) { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message, 409) { }
}

public class ServiceUnavailableException : DomainException
{
    public ServiceUnavailableException(string message = "Service temporarily unavailable.") 
        : base(message, 503) { }
}
