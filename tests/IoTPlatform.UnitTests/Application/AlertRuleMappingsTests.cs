using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests.Application;

[TestClass]
public class AlertRuleMappingsTests
{
    [TestMethod]
    public void ToDto_ValidEntity_CopiesAllFields()
    {
        var deviceAssetId = Guid.NewGuid();

        var rule = new AlertRule(
            deviceAssetId,
            "High Temperature",
            MetricType.Temperature,
            ComparisonOperator.GreaterThan,
            75m,
            AlertSeverity.Warning);

        var dto = rule.ToDto();

        Assert.AreEqual(rule.Id, dto.Id);
        Assert.AreEqual(rule.DeviceAssetId, dto.DeviceAssetId);
        Assert.AreEqual(rule.Name, dto.Name);
        Assert.AreEqual("Temperature", dto.MetricType);
        Assert.AreEqual("GreaterThan", dto.Operator);
        Assert.AreEqual(rule.Threshold, dto.Threshold);
        Assert.AreEqual("Warning", dto.Severity);
        Assert.AreEqual(rule.IsEnabled, dto.IsEnabled);
    }
}