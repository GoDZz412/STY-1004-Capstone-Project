using IoTPlatform.Domain.Entities;

namespace IoTPlatform.Application.DTOs;

public static class AlertRuleMappings
{
    public static AlertRuleDto ToDto(this AlertRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return new AlertRuleDto(
            rule.Id,
            rule.DeviceAssetId,
            rule.Name,
            rule.MetricType.ToString(),
            rule.Operator.ToString(),
            rule.Threshold,
            rule.Severity.ToString(),
            rule.IsEnabled);
    }
}