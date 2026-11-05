namespace IoTPlatform.Application.DTOs;

public sealed record DeviceAssetDto(
    Guid Id, string SerialNumber, string Name, string DeviceType, string Location, string Status,
    decimal OperatingMin, decimal OperatingMax, string OperatingUnit,
    DateTime InstalledOnUtc, DateTime CreatedAtUtc);
    