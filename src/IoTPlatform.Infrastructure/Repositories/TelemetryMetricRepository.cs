using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Repositories;

public sealed class TelemetryMetricRepository : ITelemetryMetricRepository
{
    private readonly IoTPlatformDbContext _context;

    public TelemetryMetricRepository(IoTPlatformDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TelemetryMetric metric,
        CancellationToken cancellationToken = default)
    {
        await _context.TelemetryMetrics.AddAsync(
            metric,
            cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<TelemetryMetric> metrics,
        CancellationToken cancellationToken = default)
    {
        await _context.TelemetryMetrics.AddRangeAsync(
            metrics,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TelemetryMetric>> GetForDeviceAsync(
        Guid deviceAssetId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.TelemetryMetrics
            .Where(metric =>
                metric.DeviceAssetId == deviceAssetId &&
                metric.RecordedAtUtc >= fromUtc &&
                metric.RecordedAtUtc <= toUtc)
            .OrderBy(metric => metric.RecordedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<TelemetryMetric?> GetLatestAsync(
        Guid deviceAssetId,
        MetricType metricType,
        CancellationToken cancellationToken = default)
    {
        return await _context.TelemetryMetrics
            .Where(metric =>
                metric.DeviceAssetId == deviceAssetId &&
                metric.MetricType == metricType)
            .OrderByDescending(metric => metric.RecordedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
