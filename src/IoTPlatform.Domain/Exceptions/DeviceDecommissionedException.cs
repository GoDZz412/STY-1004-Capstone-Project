namespace IoTPlatform.Domain.Exceptions;

public sealed class DeviceDecommissionedException : DomainException
{
    public DeviceDecommissionedException(Guid deviceId)
        : base($"Device {deviceId} is decommissioned and can no longer be modified.")
    {
        DeviceId = deviceId;
    }

    public Guid DeviceId { get; }
}