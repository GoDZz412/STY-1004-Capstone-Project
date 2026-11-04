using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.Abstractions.Repositories;

/// <summary>
/// Defines persistence operations for alert rules.
/// </summary>
public interface IAlertRuleRepository
{
    Task AddAsync(
        AlertRule alertRule,
        CancellationToken cancellationToken = default);
}
