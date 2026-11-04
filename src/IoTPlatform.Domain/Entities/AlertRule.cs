using IoTPlatform.Domain.Common;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Domain.Entities;

/// <summary>
/// Defines a threshold rule used to evaluate telemetry readings.
/// </summary>
public sealed class AlertRule : Entity
{
    public const int NameMaxLength = 100;

    public AlertRule(
        Guid deviceAssetId,
        string name,
        MetricType metricType,
        ComparisonOperator @operator,
        decimal threshold,
        AlertSeverity severity,
        Guid? id = null)
        : base(id ?? Guid.NewGuid())
    {
        DeviceAssetId = Guard.NotEmpty(
            deviceAssetId, nameof(DeviceAssetId));

        Name = Guard.RequiredText(
            name, nameof(Name), NameMaxLength);

        MetricType = Guard.Defined(
            metricType, nameof(MetricType));

        Operator = Guard.Defined(
            @operator, nameof(Operator));

        Threshold = threshold;

        Severity = Guard.Defined(
            severity, nameof(Severity));

        IsEnabled = true;
    }

    // Required by Entity Framework Core.
    private AlertRule() : base()
    {
        Name = string.Empty;
    }

    public Guid DeviceAssetId { get; private set; }

    public string Name { get; private set; }

    public MetricType MetricType { get; private set; }

    public ComparisonOperator Operator { get; private set; }

    public decimal Threshold { get; private set; }

    public AlertSeverity Severity { get; private set; }

    public bool IsEnabled { get; private set; }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public bool IsTriggeredBy(TelemetryMetric metric)
    {
        Guard.Required(metric, nameof(metric));

        if (!IsEnabled)
            return false;

        if (DeviceAssetId != metric.DeviceAssetId)
            return false;

        if (MetricType != metric.MetricType)
            return false;

        return Operator switch
        {
            ComparisonOperator.GreaterThan =>
                metric.Value > Threshold,

            ComparisonOperator.LessThan =>
                metric.Value < Threshold,

            _ => false
        };
    }
}
