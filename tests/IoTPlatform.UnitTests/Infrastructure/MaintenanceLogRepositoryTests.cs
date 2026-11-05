using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Repositories;
using IoTPlatform.UnitTests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public class MaintenanceLogRepositoryTests
{
    private static readonly DateTime Jan10 = new(2025, 1, 10, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Feb10 = new(2025, 2, 10, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Mar10 = new(2025, 3, 10, 8, 0, 0, DateTimeKind.Utc);

    private static DeviceAsset CreateDevice(string serial = "IOT-0001-PU") =>
        new(SerialNumber.Create(serial),
            "Pump 1", DeviceType.Pump, "Intake Station",
            new OperatingRange(0, 10, "bar"),
            new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

    private static async Task SaveAsync(SqliteTestDatabase db, params DeviceAsset[] devices)
    {
        db.Context.AddRange(devices);
        await db.Context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingLog_RoundTripsAllFields()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice();
        var log = device.LogMaintenance(MaintenanceType.PartReplacement, "Jane Doe", "Replaced the impeller", Feb10);
        await SaveAsync(db, device);

        using var read = db.CreateNewContext();
        var loaded = await new MaintenanceLogRepository(read).GetByIdAsync(log.Id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(log.Id, loaded.Id);
        Assert.AreEqual(device.Id, loaded.DeviceAssetId);
        Assert.AreEqual(MaintenanceType.PartReplacement, loaded.MaintenanceType);
        Assert.AreEqual("Jane Doe", loaded.Technician);
        Assert.AreEqual("Replaced the impeller", loaded.Description);
        Assert.AreEqual(Feb10, loaded.PerformedOnUtc);
    }

    [TestMethod]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        using var db = SqliteTestDatabase.Create();

        var loaded = await new MaintenanceLogRepository(db.Context).GetByIdAsync(Guid.NewGuid());

        Assert.IsNull(loaded);
    }

    [TestMethod]
    public async Task GetForDeviceAsync_TwoDevices_ReturnsOnlyThatDevicesLogs()
    {
        using var db = SqliteTestDatabase.Create();
        var pump = CreateDevice("IOT-0001-PU");
        var motor = CreateDevice("IOT-0003-MO");
        var pumpLog = pump.LogMaintenance(MaintenanceType.Inspection, "Jane Doe", "Pump inspection", Jan10);
        motor.LogMaintenance(MaintenanceType.Repair, "Sam Lee", "Motor repair", Feb10);
        await SaveAsync(db, pump, motor);

        using var read = db.CreateNewContext();
        var logs = await new MaintenanceLogRepository(read).GetForDeviceAsync(pump.Id);

        Assert.HasCount(1, logs);
        Assert.AreEqual(pumpLog.Id, logs[0].Id);
    }

    [TestMethod]
    public async Task GetForDeviceAsync_SeveralLogs_ReturnsNewestFirst()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice();
        var february = device.LogMaintenance(MaintenanceType.Repair, "Jane Doe", "Repair", Feb10);
        var january = device.LogMaintenance(MaintenanceType.Inspection, "Jane Doe", "Inspection", Jan10);
        var march = device.LogMaintenance(MaintenanceType.Calibration, "Jane Doe", "Calibration", Mar10);
        await SaveAsync(db, device);

        using var read = db.CreateNewContext();
        var logs = await new MaintenanceLogRepository(read).GetForDeviceAsync(device.Id);

        CollectionAssert.AreEqual(
            new[] { march.Id, february.Id, january.Id },
            logs.Select(l => l.Id).ToArray());
    }

    [TestMethod]
    public async Task DeleteDevice_WithMaintenanceLogs_ThrowsDbUpdateException()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice();
        device.LogMaintenance(MaintenanceType.Inspection, "Jane Doe", "Inspection", Jan10);
        await SaveAsync(db, device);

        // Fresh context that never loaded the logs, so the database's foreign key (Restrict) is what refuses the delete.
        using var write = db.CreateNewContext();
        var loaded = await write.DeviceAssets.SingleAsync(d => d.Id == device.Id);
        write.DeviceAssets.Remove(loaded);

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => write.SaveChangesAsync());
    }
}
