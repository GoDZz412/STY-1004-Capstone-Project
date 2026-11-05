using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Application.Abstractions.Repositories;

public interface ITelemetryMetricRepository
{
    Task AddAsync(
        TelemetryMetric metric,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TelemetryMetric> metrics,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelemetryMetric>> GetForDeviceAsync(
        Guid deviceAssetId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<TelemetryMetric?> GetLatestAsync(
        Guid deviceAssetId,
        MetricType metricType,
        CancellationToken cancellationToken = default);
}