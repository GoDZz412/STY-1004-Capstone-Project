using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests.Application;

[TestClass]
public class TelemetryMetricMappingsTests
{
    [TestMethod]
    public void ToDto_ValidEntity_CopiesAllFields()
    {
        var deviceAssetId = Guid.NewGuid();

        var recordedAtUtc = new DateTime(
            2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var metric = new TelemetryMetric(
            deviceAssetId,
            MetricType.Temperature,
            42.5m,
            "Celsius",
            recordedAtUtc);

        var dto = metric.ToDto();

        Assert.AreEqual(metric.Id, dto.Id);
        Assert.AreEqual(metric.DeviceAssetId, dto.DeviceAssetId);
        Assert.AreEqual("Temperature", dto.MetricType);
        Assert.AreEqual(metric.Value, dto.Value);
        Assert.AreEqual(metric.Unit, dto.Unit);
        Assert.AreEqual(metric.RecordedAtUtc, dto.RecordedAtUtc);
    }
}