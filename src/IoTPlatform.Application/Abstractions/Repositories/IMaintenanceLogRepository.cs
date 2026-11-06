using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.Abstractions.Repositories;

/// <summary>
/// Reads maintenance history. There is no AddAsync: logs are added through
/// <see cref="DeviceAsset.LogMaintenance"/> and saved with <see cref="IUnitOfWork"/>.
/// </summary>
public interface IMaintenanceLogRepository
{
    Task<MaintenanceLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>All logs of one device, newest first (by <c>PerformedOnUtc</c>).</summary>
    Task<IReadOnlyList<MaintenanceLog>> GetForDeviceAsync(
        Guid deviceAssetId,
        CancellationToken cancellationToken = default);
}
