using IoTPlatform.Domain.Common;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.Domain.Entities;

public sealed class DeviceAsset : Entity
{
    public const int NameMaxLength = 100;
    public const int LocationMaxLength = 150;

    public SerialNumber SerialNumber { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public DeviceType DeviceType { get; private set; }
    public string Location { get; private set; } = null!;
    public OperatingRange OperatingRange { get; private set; } = null!;
    public DateTime InstalledOnUtc { get; private set; }
    public DeviceStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private DeviceAsset() { } // for EF Core

    public DeviceAsset(SerialNumber serialNumber, string name, DeviceType deviceType,
                       string location, OperatingRange operatingRange, DateTime installedOnUtc)
        : base(Guid.NewGuid())
    {
        SerialNumber = Guard.Required(serialNumber, nameof(serialNumber));
        Name = Guard.RequiredText(name, nameof(name), NameMaxLength);
        DeviceType = Guard.Defined(deviceType, nameof(deviceType));
        Location = Guard.RequiredText(location, nameof(location), LocationMaxLength);
        OperatingRange = Guard.Required(operatingRange, nameof(operatingRange));
        InstalledOnUtc = Guard.NotInFuture(installedOnUtc, nameof(installedOnUtc));
        Status = DeviceStatus.Provisioned;
        CreatedAtUtc = DateTime.UtcNow;
    }

        private static readonly IReadOnlyDictionary<DeviceStatus, DeviceStatus[]> Transitions =
        new Dictionary<DeviceStatus, DeviceStatus[]>
        {
            [DeviceStatus.Provisioned]      = [DeviceStatus.Active, DeviceStatus.Decommissioned],
            [DeviceStatus.Active]           = [DeviceStatus.UnderMaintenance, DeviceStatus.Decommissioned],
            [DeviceStatus.UnderMaintenance] = [DeviceStatus.Active, DeviceStatus.Decommissioned],
            [DeviceStatus.Decommissioned]   = [],
        };

    public static IReadOnlyDictionary<DeviceStatus, DeviceStatus[]> AllowedTransitions => Transitions;

    public static bool CanTransition(DeviceStatus from, DeviceStatus to) =>
        Transitions.TryGetValue(from, out var allowed) && Array.IndexOf(allowed, to) >= 0;
}