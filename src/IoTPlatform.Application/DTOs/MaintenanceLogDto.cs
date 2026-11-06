namespace IoTPlatform.Application.DTOs;

public sealed record MaintenanceLogDto(
    Guid Id,
    Guid DeviceAssetId,
    string MaintenanceType,
    string Technician,
    string Description,
    DateTime PerformedOnUtc);
