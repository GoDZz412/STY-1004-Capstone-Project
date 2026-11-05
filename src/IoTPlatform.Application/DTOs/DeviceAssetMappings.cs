using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.DTOs;

public static class DeviceAssetMappings
{
    public static DeviceAssetDto ToDto(this DeviceAsset device)
    {
        ArgumentNullException.ThrowIfNull(device);

        return new DeviceAssetDto(
            device.Id,
            device.SerialNumber.Value,
            device.Name,
            device.DeviceType.ToString(),
            device.Location,
            device.Status.ToString(),
            device.OperatingRange.Min,
            device.OperatingRange.Max,
            device.OperatingRange.Unit,
            device.InstalledOnUtc,
            device.CreatedAtUtc);
    }
}
