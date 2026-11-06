using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Repositories;

public sealed class AlertRuleRepository : IAlertRuleRepository
{
    private readonly IoTPlatformDbContext _context;

    public AlertRuleRepository(IoTPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<AlertRule?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .FirstOrDefaultAsync(
                rule => rule.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AlertRule>> GetForDeviceAsync(
        Guid deviceAssetId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .Where(rule => rule.DeviceAssetId == deviceAssetId)
            .OrderBy(rule => rule.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AlertRule>> GetEnabledForMetricAsync(
        Guid deviceAssetId,
        MetricType metricType,
        CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .Where(rule =>
                rule.DeviceAssetId == deviceAssetId &&
                rule.MetricType == metricType &&
                rule.IsEnabled)
            .ToListAsync(cancellationToken);
    }
}