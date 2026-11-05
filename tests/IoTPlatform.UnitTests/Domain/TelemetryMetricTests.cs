using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class TelemetryMetricTests
{
    [TestMethod]
    public void Create_ValidMetric_Succeeds()
    {
        var deviceId = Guid.NewGuid();
        var recordedAt = DateTime.UtcNow;

        var metric = new TelemetryMetric(
            deviceId,
            MetricType.Temperature,
            25.5m,
            "Celsius",
            recordedAt);

        Assert.AreEqual(deviceId, metric.DeviceAssetId);
        Assert.AreEqual(MetricType.Temperature, metric.MetricType);
        Assert.AreEqual(25.5m, metric.Value);
        Assert.AreEqual("Celsius", metric.Unit);
        Assert.AreEqual(recordedAt, metric.RecordedAtUtc);
    }

    [TestMethod]
    public void Create_EmptyDeviceId_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new TelemetryMetric(
                Guid.Empty,
                MetricType.Temperature,
                25m,
                "Celsius",
                DateTime.UtcNow));
    }

    [TestMethod]
    public void Create_UndefinedMetricType_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new TelemetryMetric(
                Guid.NewGuid(),
                (MetricType)999,
                25m,
                "Celsius",
                DateTime.UtcNow));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_EmptyUnit_Throws(string? unit)
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new TelemetryMetric(
                Guid.NewGuid(),
                MetricType.Temperature,
                25m,
                unit!,
                DateTime.UtcNow));
    }

    [TestMethod]
    public void Create_UnitLongerThanMax_Throws()
    {
        var longUnit = new string('x', TelemetryMetric.UnitMaxLength + 1);

        Assert.ThrowsExactly<DomainValidationException>(() =>
            new TelemetryMetric(
                Guid.NewGuid(),
                MetricType.Temperature,
                25m,
                longUnit,
                DateTime.UtcNow));
    }

    [TestMethod]
    public void Create_UnitExactlyMaxLength_Succeeds()
    {
        var unit = new string('x', TelemetryMetric.UnitMaxLength);

        var metric = new TelemetryMetric(
            Guid.NewGuid(),
            MetricType.Temperature,
            25m,
            unit,
            DateTime.UtcNow);

        Assert.AreEqual(unit, metric.Unit);
    }

    [TestMethod]
    public void Create_FutureTimestampBeyondAllowedClockSkew_Throws()
    {
        var futureDate = DateTime.UtcNow
            .Add(TelemetryMetric.AllowedClockSkew)
            .AddMinutes(1);

        Assert.ThrowsExactly<DomainValidationException>(() =>
            new TelemetryMetric(
                Guid.NewGuid(),
                MetricType.Temperature,
                25m,
                "Celsius",
                futureDate));
    }

    [TestMethod]
    public void Create_FutureTimestampWithinAllowedClockSkew_Succeeds()
    {
        var futureDate = DateTime.UtcNow
            .Add(TelemetryMetric.AllowedClockSkew)
            .Subtract(TimeSpan.FromMinutes(1));

        var metric = new TelemetryMetric(
            Guid.NewGuid(),
            MetricType.Temperature,
            25m,
            "Celsius",
            futureDate);

        Assert.AreEqual(futureDate, metric.RecordedAtUtc);
    }
}
