using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.DTOs;

public static class MaintenanceLogMappings
{
    public static MaintenanceLogDto ToDto(this MaintenanceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new MaintenanceLogDto(
            log.Id,
            log.DeviceAssetId,
            log.MaintenanceType.ToString(),
            log.Technician,
            log.Description,
            log.PerformedOnUtc);
    }
}
