namespace OnlineStore.Core.Exceptions;


public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}


public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.") { }
}


public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}


public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}


public class ExternalServiceException : AppException
{
    public ExternalServiceException(string message) : base(message) { }
}
