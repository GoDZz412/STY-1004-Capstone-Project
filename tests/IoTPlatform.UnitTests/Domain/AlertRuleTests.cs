using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class AlertRuleTests
{
    private static AlertRule CreateRule(
        Guid? deviceId = null,
        MetricType metricType = MetricType.Temperature,
        ComparisonOperator comparisonOperator = ComparisonOperator.GreaterThan,
        decimal threshold = 40m)
    {
        return new AlertRule(
            deviceId ?? Guid.NewGuid(),
            "High Temperature Alert",
            metricType,
            comparisonOperator,
            threshold,
            AlertSeverity.Warning);
    }

    private static TelemetryMetric CreateMetric(
        Guid deviceId,
        decimal value,
        MetricType metricType = MetricType.Temperature)
    {
        return new TelemetryMetric(
            deviceId,
            metricType,
            value,
            "Celsius",
            DateTime.UtcNow);
    }

    [TestMethod]
    public void Create_ValidRule_Succeeds()
    {
        var deviceId = Guid.NewGuid();

        var rule = CreateRule(deviceId);

        Assert.AreEqual(deviceId, rule.DeviceAssetId);
        Assert.AreEqual("High Temperature Alert", rule.Name);
        Assert.AreEqual(40m, rule.Threshold);
        Assert.IsTrue(rule.IsEnabled);
    }

    [TestMethod]
    public void IsTriggeredBy_GreaterThan_AboveThreshold_ReturnsTrue()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(deviceId);

        var metric = CreateMetric(deviceId, 45m);

        Assert.IsTrue(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void IsTriggeredBy_LessThan_BelowThreshold_ReturnsTrue()
    {
        var deviceId = Guid.NewGuid();

        var rule = CreateRule(
            deviceId,
            comparisonOperator: ComparisonOperator.LessThan);

        var metric = CreateMetric(deviceId, 30m);

        Assert.IsTrue(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void IsTriggeredBy_EqualToThreshold_ReturnsFalse()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(deviceId);

        var metric = CreateMetric(deviceId, 40m);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void IsTriggeredBy_DifferentMetricType_ReturnsFalse()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(deviceId);

        var metric = CreateMetric(
            deviceId,
            45m,
            MetricType.Humidity);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void IsTriggeredBy_DifferentDevice_ReturnsFalse()
    {
        var rule = CreateRule(Guid.NewGuid());

        var metric = CreateMetric(Guid.NewGuid(), 45m);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void IsTriggeredBy_DisabledRule_ReturnsFalse()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(deviceId);

        rule.Disable();

        var metric = CreateMetric(deviceId, 45m);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
    }

    [TestMethod]
    public void Enable_DisabledRule_EnablesRule()
    {
        var rule = CreateRule();

        rule.Disable();
        rule.Enable();

        Assert.IsTrue(rule.IsEnabled);
    }

    [TestMethod]
    public void Create_EmptyName_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new AlertRule(
                Guid.NewGuid(),
                "",
                MetricType.Temperature,
                ComparisonOperator.GreaterThan,
                40m,
                AlertSeverity.Warning));
    }
}
