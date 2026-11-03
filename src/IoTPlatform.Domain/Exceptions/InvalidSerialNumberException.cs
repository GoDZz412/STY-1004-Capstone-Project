namespace IoTPlatform.Domain.Exceptions;

public sealed class InvalidSerialNumberException : DomainException
{
    public InvalidSerialNumberException(string? attemptedValue)
        : base($"Serial number '{attemptedValue}' is invalid. Expected format IOT-####-AA (e.g. IOT-0042-PX).")
    {
        AttemptedValue = attemptedValue;
    }

    public string? AttemptedValue { get; }
}