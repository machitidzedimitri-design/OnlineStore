namespace OnlineStore.Core.Exceptions;

/// <summary>Base type for expected, business-level errors. Mapped to HTTP codes by the middleware.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

/// <summary>Resource does not exist -> 404.</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.") { }
}

/// <summary>Invalid input data -> 400.</summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}

/// <summary>State conflict (duplicate email, insufficient stock, FK in use) -> 409.</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>External dependency (e.g. currency API) failed -> 502.</summary>
public class ExternalServiceException : AppException
{
    public ExternalServiceException(string message) : base(message) { }
}
