using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class DeviceAssetAlertRuleTests
{
    private static DeviceAsset Create() =>
        new(
            SerialNumber.Create("IOT-0042-PX"),
            "Boiler sensor",
            DeviceType.Sensor,
            "Plant 1 - Line 2",
            new OperatingRange(0, 100, "C"),
            DateTime.UtcNow.AddDays(-1));

    [TestMethod]
    public void AddAlertRule_ValidInput_AddsRuleForThisDevice()
    {
        var device = Create();

        var rule = device.AddAlertRule(
            "High Temperature",
            MetricType.Temperature,
            ComparisonOperator.GreaterThan,
            75m,
            AlertSeverity.Warning);

        Assert.IsTrue(device.AlertRules.Contains(rule));
        Assert.AreEqual(device.Id, rule.DeviceAssetId);
    }

    [TestMethod]
    public void AddAlertRule_DecommissionedDevice_ThrowsDeviceDecommissionedException()
    {
        var device = Create();
        device.Decommission();

        Assert.ThrowsExactly<DeviceDecommissionedException>(
            () => device.AddAlertRule(
                "High Temperature",
                MetricType.Temperature,
                ComparisonOperator.GreaterThan,
                75m,
                AlertSeverity.Warning));
    }
}