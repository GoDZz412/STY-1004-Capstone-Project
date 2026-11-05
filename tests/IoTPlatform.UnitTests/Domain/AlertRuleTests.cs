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
        decimal threshold = 40m,
        string name = "High Temperature Alert",
        AlertSeverity severity = AlertSeverity.Warning)
    {
        return new AlertRule(
            deviceId ?? Guid.NewGuid(),
            name,
            metricType,
            comparisonOperator,
            threshold,
            severity);
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
    public void Create_EmptyDeviceId_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(Guid.Empty));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_InvalidName_Throws(string? name)
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(name: name!));
    }

    [TestMethod]
    public void Create_NameLongerThanMax_Throws()
    {
        var longName = new string('x', AlertRule.NameMaxLength + 1);

        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(name: longName));
    }

    [TestMethod]
    public void Create_NameExactlyMaxLength_Succeeds()
    {
        var name = new string('x', AlertRule.NameMaxLength);

        var rule = CreateRule(name: name);

        Assert.AreEqual(name, rule.Name);
    }

    [TestMethod]
    public void Create_UndefinedMetricType_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(metricType: (MetricType)999));
    }

    [TestMethod]
    public void Create_UndefinedComparisonOperator_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(comparisonOperator: (ComparisonOperator)999));
    }

    [TestMethod]
    public void Create_UndefinedSeverity_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateRule(severity: (AlertSeverity)999));
    }

    [TestMethod]
    public void IsTriggeredBy_NullMetric_Throws()
    {
        var rule = CreateRule();

        Assert.ThrowsExactly<DomainValidationException>(() =>
            rule.IsTriggeredBy(null!));
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
    public void IsTriggeredBy_GreaterThan_BelowThreshold_ReturnsFalse()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(deviceId);
        var metric = CreateMetric(deviceId, 35m);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
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
    public void IsTriggeredBy_LessThan_AboveThreshold_ReturnsFalse()
    {
        var deviceId = Guid.NewGuid();
        var rule = CreateRule(
            deviceId,
            comparisonOperator: ComparisonOperator.LessThan);
        var metric = CreateMetric(deviceId, 45m);

        Assert.IsFalse(rule.IsTriggeredBy(metric));
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
        var metric = CreateMetric(deviceId, 45m, MetricType.Humidity);

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
    public void Disable_EnabledRule_DisablesRule()
    {
        var rule = CreateRule();

        rule.Disable();

        Assert.IsFalse(rule.IsEnabled);
    }
}
