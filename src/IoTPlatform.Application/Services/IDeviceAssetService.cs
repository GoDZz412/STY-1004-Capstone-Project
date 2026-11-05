using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Application.Services;

public interface IDeviceAssetService
{
    Task<DeviceAssetDto> RegisterAsync(string serialNumber, string name, DeviceType deviceType, string location,
        decimal operatingMin, decimal operatingMax, string operatingUnit, DateTime installedOnUtc,
        CancellationToken cancellationToken = default);

    Task<DeviceAssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DeviceAssetDto> ChangeStatusAsync(Guid id, DeviceStatus newStatus,
        CancellationToken cancellationToken = default);
}
