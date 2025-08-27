namespace Sivar.Erp.Core.Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException() : base()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when business validation rules are violated
/// </summary>
public class BusinessValidationException : Exception
{
    public BusinessValidationException() : base()
    {
    }

    public BusinessValidationException(string message) : base(message)
    {
    }

    public BusinessValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
