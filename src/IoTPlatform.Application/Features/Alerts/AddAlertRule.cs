using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Application.Features.Alerts;

/// <summary>
/// Application use case for creating and saving an alert rule.
/// </summary>
public sealed class AddAlertRule
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAlertRule(
        IAlertRuleRepository alertRuleRepository,
        IUnitOfWork unitOfWork)
    {
        _alertRuleRepository = alertRuleRepository
            ?? throw new ArgumentNullException(nameof(alertRuleRepository));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AlertRule> ExecuteAsync(
        Guid deviceAssetId,
        string name,
        MetricType metricType,
        ComparisonOperator comparisonOperator,
        decimal threshold,
        AlertSeverity severity,
        CancellationToken cancellationToken = default)
    {
        var alertRule = new AlertRule(
            deviceAssetId,
            name,
            metricType,
            comparisonOperator,
            threshold,
            severity);

        await _alertRuleRepository.AddAsync(
            alertRule,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return alertRule;
    }
}
