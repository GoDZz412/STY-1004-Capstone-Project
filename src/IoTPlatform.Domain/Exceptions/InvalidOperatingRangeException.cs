namespace IoTPlatform.Domain.Exceptions;

public sealed class InvalidOperatingRangeException : DomainException
{
    public InvalidOperatingRangeException(string message) : base(message) { }
}