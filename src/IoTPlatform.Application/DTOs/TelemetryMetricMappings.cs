using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.DTOs;

public static class TelemetryMetricMappings
{
    public static TelemetryMetricDto ToDto(this TelemetryMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        return new TelemetryMetricDto(
            metric.Id,
            metric.DeviceAssetId,
            metric.MetricType.ToString(),
            metric.Value,
            metric.Unit,
            metric.RecordedAtUtc);
    }
}