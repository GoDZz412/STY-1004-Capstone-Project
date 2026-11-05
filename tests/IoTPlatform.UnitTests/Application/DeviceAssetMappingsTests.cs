using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Application;

[TestClass]
public class DeviceAssetMappingsTests
{
    private const string SerialValue = "IOT-0001-PU";
    private const string DeviceName = "Pump 1";
    private const string DeviceLocation = "Hall A";
    private const string RangeUnit = "Celsius";
    private const decimal RangeMin = 5m;
    private const decimal RangeMax = 80m;

    [TestMethod]
    public void ToDto_ValidEntity_CopiesAllFields()
    {
        var installedOn = DateTime.UtcNow.AddDays(-30);
        var device = new DeviceAsset(
            SerialNumber.Create(SerialValue),
            DeviceName,
            DeviceType.Pump,
            DeviceLocation,
            new OperatingRange(RangeMin, RangeMax, RangeUnit),
            installedOn);

        var dto = device.ToDto();

        Assert.AreEqual(device.Id, dto.Id);
        Assert.AreEqual(SerialValue, dto.SerialNumber);
        Assert.AreEqual(DeviceName, dto.Name);
        Assert.AreEqual(nameof(DeviceType.Pump), dto.DeviceType);
        Assert.AreEqual(DeviceLocation, dto.Location);
        Assert.AreEqual(nameof(DeviceStatus.Provisioned), dto.Status);
        Assert.AreEqual(RangeMin, dto.OperatingMin);
        Assert.AreEqual(RangeMax, dto.OperatingMax);
        Assert.AreEqual(RangeUnit, dto.OperatingUnit);
        Assert.AreEqual(installedOn, dto.InstalledOnUtc);
        Assert.AreEqual(device.CreatedAtUtc, dto.CreatedAtUtc);
    }
}
