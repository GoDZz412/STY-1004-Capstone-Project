using IoTPlatform.Domain.Entities;      // <-- check: where DeviceAsset lives
using IoTPlatform.Domain.Enums;         // <-- check: where DeviceStatus lives
using IoTPlatform.Domain.ValueObjects;  // <-- check: where SerialNumber lives

namespace IoTPlatform.Application.Abstractions.Repositories;

public interface IDeviceAssetRepository
{
    Task<DeviceAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceAsset?> GetBySerialNumberAsync(SerialNumber serialNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceAsset>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceAsset>> ListByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySerialNumberAsync(SerialNumber serialNumber, CancellationToken cancellationToken = default);
    Task AddAsync(DeviceAsset device, CancellationToken cancellationToken = default);
    void Update(DeviceAsset device);
    void Remove(DeviceAsset device);
}
