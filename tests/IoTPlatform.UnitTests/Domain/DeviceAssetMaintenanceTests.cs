using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class DeviceAssetMaintenanceTests
{
    private static DeviceAsset CreateDevice() =>
        new(SerialNumber.Create("IOT-0001-PU"),
            "Pump 1", DeviceType.Pump, "Intake Station",
            new OperatingRange(0, 10, "bar"),
            DateTime.UtcNow.AddDays(-30));

    [TestMethod]
    public void LogMaintenance_ValidInput_AddsLogForThisDevice()
    {
        var device = CreateDevice();
        var performedOnUtc = DateTime.UtcNow.AddDays(-1);

        var log = device.LogMaintenance(MaintenanceType.Inspection, "Jane Doe", "Quarterly pump inspection", performedOnUtc);

        Assert.HasCount(1, device.MaintenanceLogs);
        Assert.Contains(log, device.MaintenanceLogs);
        Assert.AreEqual(device.Id, log.DeviceAssetId);
        Assert.AreEqual(MaintenanceType.Inspection, log.MaintenanceType);
        Assert.AreEqual("Jane Doe", log.Technician);
        Assert.AreEqual("Quarterly pump inspection", log.Description);
        Assert.AreEqual(performedOnUtc, log.PerformedOnUtc);
    }

    [TestMethod]
    public void LogMaintenance_DecommissionedDevice_ThrowsDeviceDecommissionedException()
    {
        var device = CreateDevice();
        device.Decommission();

        Assert.ThrowsExactly<DeviceDecommissionedException>(() =>
            device.LogMaintenance(MaintenanceType.Repair, "Jane Doe", "Replace seal", DateTime.UtcNow.AddDays(-1)));
        Assert.IsEmpty(device.MaintenanceLogs);
    }
}
