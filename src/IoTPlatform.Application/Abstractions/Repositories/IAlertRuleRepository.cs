using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Application.Abstractions.Repositories;

public interface IAlertRuleRepository
{
    Task<AlertRule?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertRule>> GetForDeviceAsync(
        Guid deviceAssetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertRule>> GetEnabledForMetricAsync(
        Guid deviceAssetId,
        MetricType metricType,
        CancellationToken cancellationToken = default);
}