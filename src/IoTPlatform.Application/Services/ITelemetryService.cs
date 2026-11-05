using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Application.Services;

public interface ITelemetryService
{
    Task<TelemetryMetricDto> RecordAsync(
        Guid deviceAssetId,
        MetricType metricType,
        decimal value,
        string unit,
        DateTime recordedAtUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelemetryMetricDto>> GetForDeviceAsync(
        Guid deviceAssetId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}