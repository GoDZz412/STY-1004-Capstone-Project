namespace IoTPlatform.Application.DTOs;

public sealed record TelemetryMetricDto(
    Guid Id,
    Guid DeviceAssetId,
    string MetricType,
    decimal Value,
    string Unit,
    DateTime RecordedAtUtc);