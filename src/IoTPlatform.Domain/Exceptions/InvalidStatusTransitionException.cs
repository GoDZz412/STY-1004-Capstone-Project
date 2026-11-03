using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Domain.Exceptions;

public sealed class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(DeviceStatus from, DeviceStatus to)
        : base($"A device cannot change status from {from} to {to}.")
    {
        From = from;
        To = to;
    }

    public DeviceStatus From { get; }
    public DeviceStatus To { get; }
}