using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class DeviceAssetTests
{
    private static DeviceAsset Create(
        string name = "Boiler sensor",
        string location = "Plant 1 - Line 2",
        DateTime? installedOnUtc = null,
        DeviceType deviceType = DeviceType.Sensor) =>
        new(SerialNumber.Create("IOT-0042-PX"),
            name, deviceType, location,
            new OperatingRange(0, 100, "C"),
            installedOnUtc ?? DateTime.UtcNow.AddDays(-1));

    // ---- creation ----

    [TestMethod]
    public void Constructor_WithValidData_StartsProvisioned()
    {
        var device = Create();

        Assert.AreEqual(DeviceStatus.Provisioned, device.Status);
        Assert.AreNotEqual(Guid.Empty, device.Id);
        Assert.AreEqual("Boiler sensor", device.Name);
        Assert.AreEqual("Plant 1 - Line 2", device.Location);
    }

    [TestMethod]
    public void Constructor_SetsCreatedAtUtcToNow()
    {
        var before = DateTime.UtcNow;
        var device = Create();
        Assert.IsTrue(device.CreatedAtUtc >= before && device.CreatedAtUtc <= DateTime.UtcNow);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_WithBlankName_Throws(string? name) =>
        Assert.ThrowsExactly<DomainValidationException>(() => Create(name: name!));

    [TestMethod]
    public void Constructor_WithTooLongName_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(
            () => Create(name: new string('a', DeviceAsset.NameMaxLength + 1)));

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_WithBlankLocation_Throws(string? location) =>
        Assert.ThrowsExactly<DomainValidationException>(() => Create(location: location!));

    [TestMethod]
    public void Constructor_WithTooLongLocation_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(
            () => Create(location: new string('a', DeviceAsset.LocationMaxLength + 1)));

    [TestMethod]
    public void Constructor_WithFutureInstallDate_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(
            () => Create(installedOnUtc: DateTime.UtcNow.AddDays(1)));

    [TestMethod]
    public void Constructor_WithUndefinedDeviceType_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(() => Create(deviceType: (DeviceType)999));

    [TestMethod]
    public void Constructor_WithNullSerialNumber_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new DeviceAsset(null!, "n", DeviceType.Sensor, "l",
                            new OperatingRange(0, 100, "C"), DateTime.UtcNow.AddDays(-1)));

    [TestMethod]
    public void Constructor_WithNullOperatingRange_Throws() =>
        Assert.ThrowsExactly<DomainValidationException>(() =>
            new DeviceAsset(SerialNumber.Create("IOT-0042-PX"), "n", DeviceType.Sensor, "l",
                            null!, DateTime.UtcNow.AddDays(-1)));
    
        // ---- lifecycle table (PHASE_1 §3.4, 4x4 = 16 cells) ----

    [TestMethod]
    // from Provisioned
    [DataRow(DeviceStatus.Provisioned,      DeviceStatus.Provisioned,      false)]
    [DataRow(DeviceStatus.Provisioned,      DeviceStatus.Active,           true)]
    [DataRow(DeviceStatus.Provisioned,      DeviceStatus.UnderMaintenance, false)]
    [DataRow(DeviceStatus.Provisioned,      DeviceStatus.Decommissioned,   true)]
    // from Active
    [DataRow(DeviceStatus.Active,           DeviceStatus.Provisioned,      false)]
    [DataRow(DeviceStatus.Active,           DeviceStatus.Active,           false)]
    [DataRow(DeviceStatus.Active,           DeviceStatus.UnderMaintenance, true)]
    [DataRow(DeviceStatus.Active,           DeviceStatus.Decommissioned,   true)]
    // from UnderMaintenance
    [DataRow(DeviceStatus.UnderMaintenance, DeviceStatus.Provisioned,      false)]
    [DataRow(DeviceStatus.UnderMaintenance, DeviceStatus.Active,           true)]
    [DataRow(DeviceStatus.UnderMaintenance, DeviceStatus.UnderMaintenance, false)]
    [DataRow(DeviceStatus.UnderMaintenance, DeviceStatus.Decommissioned,   true)]
    // from Decommissioned
    [DataRow(DeviceStatus.Decommissioned,   DeviceStatus.Provisioned,      false)]
    [DataRow(DeviceStatus.Decommissioned,   DeviceStatus.Active,           false)]
    [DataRow(DeviceStatus.Decommissioned,   DeviceStatus.UnderMaintenance, false)]
    [DataRow(DeviceStatus.Decommissioned,   DeviceStatus.Decommissioned,   false)]
    public void CanTransition_MatchesLifecycleTable(DeviceStatus from, DeviceStatus to, bool expected) =>
        Assert.AreEqual(expected, DeviceAsset.CanTransition(from, to));

       [TestMethod]
    public void AllowedTransitions_ContainsEveryStatus() =>
        Assert.HasCount(Enum.GetValues<DeviceStatus>().Length, DeviceAsset.AllowedTransitions);
}