using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Repositories;
using IoTPlatform.UnitTests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public class DeviceAssetRepositoryTests
{
    private const string SerialA = "IOT-0001-PU";
    private const string SerialB = "IOT-0002-PU";
    private const string UnknownSerial = "IOT-9999-ZZ";
    private const string DefaultLocation = "Hall A";
    private const string DefaultUnit = "Celsius";
    private const decimal DefaultMin = 0m;
    private const decimal DefaultMax = 100m;
    private const decimal SmallMax = 9m;
    private const decimal TwoDigitMax = 10m;

    private static DeviceAsset CreateDevice(
        string serial = SerialA,
        string name = "Pump 1",
        decimal min = DefaultMin,
        decimal max = DefaultMax)
    {
        return new DeviceAsset(
            SerialNumber.Create(serial),
            name,
            Enum.GetValues<DeviceType>().First(),
            DefaultLocation,
            new OperatingRange(min, max, DefaultUnit),
            DateTime.UtcNow.AddDays(-30));
    }

    [TestMethod]
    public async Task AddAsync_ThenGetByIdAsync_RoundTripsAllFields()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice();
        await new DeviceAssetRepository(db.Context).AddAsync(device);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var loaded = await new DeviceAssetRepository(read).GetByIdAsync(device.Id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(device.Id, loaded.Id);
        Assert.AreEqual(device.SerialNumber, loaded.SerialNumber);
        Assert.AreEqual(device.Name, loaded.Name);
        Assert.AreEqual(device.DeviceType, loaded.DeviceType);
        Assert.AreEqual(device.Location, loaded.Location);
        Assert.AreEqual(device.OperatingRange, loaded.OperatingRange);
        Assert.AreEqual(device.InstalledOnUtc, loaded.InstalledOnUtc);
        Assert.AreEqual(device.Status, loaded.Status);
        Assert.AreEqual(device.CreatedAtUtc, loaded.CreatedAtUtc);
        Assert.IsEmpty(loaded.AlertRules);
        Assert.IsEmpty(loaded.MaintenanceLogs);
    }

    [TestMethod]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        using var db = SqliteTestDatabase.Create();

        var loaded = await new DeviceAssetRepository(db.Context).GetByIdAsync(Guid.NewGuid());

        Assert.IsNull(loaded);
    }

    [TestMethod]
    public async Task GetBySerialNumberAsync_ExistingSerial_ReturnsDevice()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice();
        await new DeviceAssetRepository(db.Context).AddAsync(device);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var loaded = await new DeviceAssetRepository(read).GetBySerialNumberAsync(SerialNumber.Create(SerialA));

        Assert.IsNotNull(loaded);
        Assert.AreEqual(device.Id, loaded.Id);
    }

    [TestMethod]
    public async Task ExistsBySerialNumberAsync_ExistingSerial_ReturnsTrue()
    {
        using var db = SqliteTestDatabase.Create();
        await new DeviceAssetRepository(db.Context).AddAsync(CreateDevice());
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var exists = await new DeviceAssetRepository(read).ExistsBySerialNumberAsync(SerialNumber.Create(SerialA));

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task ExistsBySerialNumberAsync_UnknownSerial_ReturnsFalse()
    {
        using var db = SqliteTestDatabase.Create();
        await new DeviceAssetRepository(db.Context).AddAsync(CreateDevice());
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var exists = await new DeviceAssetRepository(read).ExistsBySerialNumberAsync(SerialNumber.Create(UnknownSerial));

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task SaveChanges_DuplicateSerialNumber_ThrowsDbUpdateException()
    {
        using var db = SqliteTestDatabase.Create();
        var repo = new DeviceAssetRepository(db.Context);
        await repo.AddAsync(CreateDevice(SerialA, "Pump 1"));
        await db.Context.SaveChangesAsync();
        await repo.AddAsync(CreateDevice(SerialA, "Pump 2"));

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.Context.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ListByStatusAsync_ReturnsOnlyMatchingStatus()
    {
        using var db = SqliteTestDatabase.Create();
        var provisioned = CreateDevice(SerialA, "Pump 1");
        var active = CreateDevice(SerialB, "Pump 2");
        active.Activate();
        var repo = new DeviceAssetRepository(db.Context);
        await repo.AddAsync(provisioned);
        await repo.AddAsync(active);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var result = await new DeviceAssetRepository(read).ListByStatusAsync(DeviceStatus.Active);

        Assert.HasCount(1, result);
        Assert.AreEqual(active.Id, result[0].Id);
    }

    [TestMethod]
    public async Task ListAsync_MultipleDevices_ReturnsOrderedByName()
    {
        using var db = SqliteTestDatabase.Create();
        var repo = new DeviceAssetRepository(db.Context);
        await repo.AddAsync(CreateDevice(SerialA, "Zeta"));
        await repo.AddAsync(CreateDevice(SerialB, "Alpha"));
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var result = await new DeviceAssetRepository(read).ListAsync();

        Assert.HasCount(2, result);
        Assert.AreEqual("Alpha", result[0].Name);
        Assert.AreEqual("Zeta", result[1].Name);
    }

    [TestMethod]
    public async Task AddAsync_RangeNineToTen_IsAccepted()
    {
        using var db = SqliteTestDatabase.Create();
        var device = CreateDevice(min: SmallMax, max: TwoDigitMax);
        await new DeviceAssetRepository(db.Context).AddAsync(device);

        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();
        var loaded = await new DeviceAssetRepository(read).GetByIdAsync(device.Id);
        Assert.IsNotNull(loaded);
        Assert.AreEqual(SmallMax, loaded.OperatingRange.Min);
        Assert.AreEqual(TwoDigitMax, loaded.OperatingRange.Max);
    }
}
