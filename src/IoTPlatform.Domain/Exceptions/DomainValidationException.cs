namespace IoTPlatform.Domain.Exceptions;

/// <summary>Generic validation failure: empty name, too long, future date, empty Guid, ...</summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}