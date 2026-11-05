
using IoTPlatform.Domain.Common;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.Domain.Entities;

/// <summary>
/// Represents a sensor reading received from an IoT device.
/// </summary>
public sealed class TelemetryMetric : Entity
{
    public const int UnitMaxLength = 20;

    public static readonly TimeSpan AllowedClockSkew =
        TimeSpan.FromMinutes(5);

    public TelemetryMetric(
        Guid deviceAssetId,
        MetricType metricType,
        decimal value,
        string unit,
        DateTime recordedAtUtc)
        : base(Guid.NewGuid())
    {
        DeviceAssetId = Guard.NotEmpty(
            deviceAssetId, nameof(DeviceAssetId));

        MetricType = Guard.Defined(
            metricType, nameof(MetricType));

        Value = value;

        Unit = Guard.RequiredText(
            unit, nameof(Unit), UnitMaxLength);

        RecordedAtUtc = Guard.NotInFuture(
            recordedAtUtc,
            nameof(RecordedAtUtc),
            AllowedClockSkew);
    }

    // Required by Entity Framework Core.
    private TelemetryMetric() : base()
    {
        Unit = string.Empty;
    }

    public Guid DeviceAssetId { get; private set; }

    public MetricType MetricType { get; private set; }

    public decimal Value { get; private set; }

    public string Unit { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }
}
