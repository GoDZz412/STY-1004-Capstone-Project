using IoTPlatform.Domain.Common;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Domain.Entities;

/// <summary>
/// A historical record of maintenance work performed on a device. Immutable once created.
/// </summary>
public sealed class MaintenanceLog : Entity
{
    public const int TechnicianMaxLength = 100;
    public const int DescriptionMaxLength = 1000;

    public MaintenanceLog(
        Guid deviceAssetId,
        MaintenanceType maintenanceType,
        string technician,
        string description,
        DateTime performedOnUtc)
        : base(Guid.NewGuid())
    {
        DeviceAssetId = Guard.NotEmpty(
            deviceAssetId, nameof(DeviceAssetId));

        MaintenanceType = Guard.Defined(
            maintenanceType, nameof(MaintenanceType));

        Technician = Guard.RequiredText(
            technician, nameof(Technician), TechnicianMaxLength);

        Description = Guard.RequiredText(
            description, nameof(Description), DescriptionMaxLength);

        PerformedOnUtc = Guard.NotInFuture(
            performedOnUtc, nameof(PerformedOnUtc));
    }

    // Required by Entity Framework Core.
    private MaintenanceLog() : base()
    {
        Technician = string.Empty;
        Description = string.Empty;
    }

    public Guid DeviceAssetId { get; private set; }

    public MaintenanceType MaintenanceType { get; private set; }

    public string Technician { get; private set; }

    public string Description { get; private set; }

    public DateTime PerformedOnUtc { get; private set; }
}
