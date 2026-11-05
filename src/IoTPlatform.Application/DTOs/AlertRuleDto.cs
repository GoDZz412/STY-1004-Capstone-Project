namespace IoTPlatform.Application.DTOs;

public sealed record AlertRuleDto(
    Guid Id,
    Guid DeviceAssetId,
    string Name,
    string MetricType,
    string Operator,
    decimal Threshold,
    string Severity,
    bool IsEnabled);