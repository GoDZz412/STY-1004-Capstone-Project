using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Repositories;
using IoTPlatform.UnitTests.TestHelpers;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public sealed class AlertRuleRepositoryTests
{
    private static DeviceAsset CreateDevice(
        string serialNumber = "IOT-0001-PU",
        string name = "Pump 1") =>
        new(
            SerialNumber.Create(serialNumber),
            name,
            DeviceType.Pump,
            "Intake Station",
            new OperatingRange(0, 10, "bar"),
            DateTime.UtcNow.AddDays(-30));

    [TestMethod]
    public async Task GetByIdAsync_ExistingRule_ReturnsAllFields()
    {
        using var db = SqliteTestDatabase.Create();

        var device = CreateDevice();

        var rule = device.AddAlertRule(
            "High pressure",
            MetricType.Pressure,
            ComparisonOperator.GreaterThan,
            9m,
            AlertSeverity.Critical);

        db.Context.Add(device);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var result = await new AlertRuleRepository(read)
            .GetByIdAsync(rule.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(rule.Id, result.Id);
        Assert.AreEqual(rule.DeviceAssetId, result.DeviceAssetId);
        Assert.AreEqual(rule.Name, result.Name);
        Assert.AreEqual(rule.MetricType, result.MetricType);
        Assert.AreEqual(rule.Operator, result.Operator);
        Assert.AreEqual(rule.Threshold, result.Threshold);
        Assert.AreEqual(rule.Severity, result.Severity);
        Assert.AreEqual(rule.IsEnabled, result.IsEnabled);
    }

    [TestMethod]
    public async Task GetForDeviceAsync_MultipleDevices_ReturnsOnlyRequestedDeviceRules()
    {
        using var db = SqliteTestDatabase.Create();

        var firstDevice = CreateDevice(
            "IOT-0001-PU",
            "Pump 1");

        var secondDevice = CreateDevice(
            "IOT-0002-PU",
            "Pump 2");

        firstDevice.AddAlertRule(
            "Z - High pressure",
            MetricType.Pressure,
            ComparisonOperator.GreaterThan,
            9m,
            AlertSeverity.Critical);

        firstDevice.AddAlertRule(
            "A - Low pressure",
            MetricType.Pressure,
            ComparisonOperator.LessThan,
            2m,
            AlertSeverity.Warning);

        secondDevice.AddAlertRule(
            "Other pump rule",
            MetricType.Pressure,
            ComparisonOperator.GreaterThan,
            8m,
            AlertSeverity.Info);

        db.Context.AddRange(firstDevice, secondDevice);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var results = await new AlertRuleRepository(read)
            .GetForDeviceAsync(firstDevice.Id);

        Assert.HasCount(2, results);
        Assert.AreEqual(firstDevice.Id, results[0].DeviceAssetId);
        Assert.AreEqual(firstDevice.Id, results[1].DeviceAssetId);
        Assert.AreEqual("A - Low pressure", results[0].Name);
        Assert.AreEqual("Z - High pressure", results[1].Name);
    }

    [TestMethod]
    public async Task GetEnabledForMetricAsync_DisabledAndOtherMetricRules_SkipsThem()
    {
        using var db = SqliteTestDatabase.Create();

        var device = CreateDevice();

        var enabledPressureRule = device.AddAlertRule(
            "Enabled pressure",
            MetricType.Pressure,
            ComparisonOperator.GreaterThan,
            9m,
            AlertSeverity.Critical);

        var disabledPressureRule = device.AddAlertRule(
            "Disabled pressure",
            MetricType.Pressure,
            ComparisonOperator.LessThan,
            2m,
            AlertSeverity.Warning);

        disabledPressureRule.Disable();

        device.AddAlertRule(
            "Temperature rule",
            MetricType.Temperature,
            ComparisonOperator.GreaterThan,
            80m,
            AlertSeverity.Info);

        db.Context.Add(device);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var results = await new AlertRuleRepository(read)
            .GetEnabledForMetricAsync(
                device.Id,
                MetricType.Pressure);

        Assert.HasCount(1, results);
        Assert.AreEqual(enabledPressureRule.Id, results[0].Id);
        Assert.AreEqual(MetricType.Pressure, results[0].MetricType);
        Assert.IsTrue(results[0].IsEnabled);
    }
}