using IoTPlatform.Domain.Common;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.Domain.Entities;

public sealed class DeviceAsset : Entity
{
    public const int NameMaxLength = 100;
    public const int LocationMaxLength = 150;
    private readonly List<AlertRule> _alertRules = new();
    private readonly List<MaintenanceLog> _maintenanceLogs = new();

    public SerialNumber SerialNumber { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public DeviceType DeviceType { get; private set; }
    public string Location { get; private set; } = null!;
    public OperatingRange OperatingRange { get; private set; } = null!;
    public DateTime InstalledOnUtc { get; private set; }
    public DeviceStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<AlertRule> AlertRules => _alertRules.AsReadOnly();
    public IReadOnlyCollection<MaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

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
    
        public void ChangeStatus(DeviceStatus newStatus)
    {
        Guard.Defined(newStatus, nameof(newStatus));
        if (!CanTransition(Status, newStatus))
            throw new InvalidStatusTransitionException(Status, newStatus);
        Status = newStatus;
    }

    public void Activate() => ChangeStatus(DeviceStatus.Active);
    public void StartMaintenance() => ChangeStatus(DeviceStatus.UnderMaintenance);
    public void CompleteMaintenance() => ChangeStatus(DeviceStatus.Active);
    public void Decommission() => ChangeStatus(DeviceStatus.Decommissioned);

        public void Rename(string newName)
    {
        EnsureNotDecommissioned();
        Name = Guard.RequiredText(newName, nameof(newName), NameMaxLength);
    }

    public void Relocate(string newLocation)
    {
        EnsureNotDecommissioned();
        Location = Guard.RequiredText(newLocation, nameof(newLocation), LocationMaxLength);
    }

    public void UpdateOperatingRange(OperatingRange newRange)
    {
        EnsureNotDecommissioned();
        OperatingRange = Guard.Required(newRange, nameof(newRange));
    }

    // Private per PHASE_1 §3.6. B's AddAlertRule and C's LogMaintenance live in this class and call it first.
    private void EnsureNotDecommissioned()
    {
        if (Status == DeviceStatus.Decommissioned)
            throw new DeviceDecommissionedException(Id);
    }

        // ── B: AddAlertRule goes here (P1-W2-B2) ──

    // ── C: LogMaintenance goes here (P1-W2-C3) ──
}
