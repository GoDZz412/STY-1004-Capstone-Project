using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Infrastructure.Seeding;
using IoTPlatform.UnitTests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.Infrastructure;

/// <summary>
/// Runs the loader against the REAL seed files (copied to the test output), so a mistake in any team member's JSON fails here.
/// </summary>
[TestClass]
public class JsonSeedLoaderTests
{
    private const int ExpectedDevices = 8;
    private const int ExpectedAlertRules = 10;
    private const int ExpectedDisabledRules = 1;
    private const int ExpectedMaintenanceLogs = 8;
    private const int ExpectedReadings = 50;

    private static async Task AssertSeedCountsAsync(IoTPlatformDbContext context)
    {
        Assert.AreEqual(ExpectedDevices, await context.DeviceAssets.CountAsync());
        Assert.AreEqual(ExpectedAlertRules, await context.AlertRules.CountAsync());
        Assert.AreEqual(ExpectedDisabledRules, await context.AlertRules.CountAsync(r => !r.IsEnabled));
        Assert.AreEqual(ExpectedMaintenanceLogs, await context.MaintenanceLogs.CountAsync());
        Assert.AreEqual(ExpectedReadings, await context.TelemetryMetrics.CountAsync());
    }

    [TestMethod]
    public async Task SeedAsync_EmptyDatabase_LoadsAllSeedFiles()
    {
        using var db = SqliteTestDatabase.Create();

        await new JsonSeedLoader().SeedAsync(db.Context);

        using var read = db.CreateNewContext();
        await AssertSeedCountsAsync(read);
    }

    [TestMethod]
    public void Seed_EmptyDatabase_LoadsAllSeedFiles()
    {
        using var db = SqliteTestDatabase.Create();

        new JsonSeedLoader().Seed(db.Context);

        using var read = db.CreateNewContext();
        AssertSeedCountsAsync(read).GetAwaiter().GetResult();
    }

    [TestMethod]
    [DataRow("IOT-0001-PU", DeviceStatus.Active)]
    [DataRow("IOT-0002-PU", DeviceStatus.UnderMaintenance)]
    [DataRow("IOT-0006-SE", DeviceStatus.Provisioned)]
    [DataRow("IOT-0008-PU", DeviceStatus.Decommissioned)]
    public async Task SeedAsync_EmptyDatabase_SetsEachDeviceStatus(string serial, DeviceStatus expectedStatus)
    {
        using var db = SqliteTestDatabase.Create();

        await new JsonSeedLoader().SeedAsync(db.Context);

        using var read = db.CreateNewContext();
        var serialNumber = SerialNumber.Create(serial);
        var device = await read.DeviceAssets.SingleAsync(d => d.SerialNumber == serialNumber);
        Assert.AreEqual(expectedStatus, device.Status);
    }

    [TestMethod]
    public async Task SeedAsync_DecommissionedDevice_KeepsRulesAndHistoryAddedBeforeDecommissioning()
    {
        using var db = SqliteTestDatabase.Create();

        await new JsonSeedLoader().SeedAsync(db.Context);

        using var read = db.CreateNewContext();
        var serialNumber = SerialNumber.Create("IOT-0008-PU");
        var device = await read.DeviceAssets
            .Include(d => d.AlertRules)
            .Include(d => d.MaintenanceLogs)
            .SingleAsync(d => d.SerialNumber == serialNumber);
        Assert.IsNotEmpty(device.AlertRules);
        Assert.IsNotEmpty(device.MaintenanceLogs);
    }

    [TestMethod]
    public async Task SeedAsync_RunTwice_KeepsRowCounts()
    {
        using var db = SqliteTestDatabase.Create();
        var loader = new JsonSeedLoader();

        await loader.SeedAsync(db.Context);
        using (var second = db.CreateNewContext())
        {
            await loader.SeedAsync(second);
        }

        using var read = db.CreateNewContext();
        await AssertSeedCountsAsync(read);
    }

    // Each row breaks one record in a temporary copy of the real files.
    [TestMethod]
    [DataRow("devices.json", "\"deviceType\": \"Pump\"", "\"deviceType\": \"pump\"")]            // enum not spelled like C#
    [DataRow("devices.json", "\"name\": \"Plant Network Gateway\"", "\"name\": \"   \"")]          // domain rule: name required
    [DataRow("alert-rules.json", "\"serialNumber\": \"IOT-0001-PU\"", "\"serialNumber\": \"IOT-9999-ZZ\"")] // unknown device
    [DataRow("maintenance-logs.json", "\"maintenanceType\": \"Repair\"", "\"maintenanceType\": \"3\"")] // number instead of name
    [DataRow("telemetry.json", "\"unit\": \"bar\"", "\"unit\": \"\"")]                             // domain rule: unit required
    public async Task SeedAsync_BrokenRecord_ThrowsNamingFileAndSavesNothing(string fileName, string original, string broken)
    {
        var folder = CopySeedFilesToTempFolder();
        try
        {
            var path = Path.Combine(folder, fileName);
            var json = await File.ReadAllTextAsync(path);
            Assert.Contains(original, json, $"Test setup: '{original}' not found in {fileName}.");
            await File.WriteAllTextAsync(path, ReplaceFirst(json, original, broken));
            using var db = SqliteTestDatabase.Create();

            var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => new JsonSeedLoader(folder).SeedAsync(db.Context));

            Assert.Contains(fileName, ex.Message);
            Assert.Contains("record", ex.Message);
            using var read = db.CreateNewContext();
            Assert.AreEqual(0, await read.DeviceAssets.CountAsync());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static string CopySeedFilesToTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "iot-seed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        foreach (var file in Directory.GetFiles(JsonSeedLoader.DefaultDataDirectory, "*.json"))
        {
            File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));
        }
        return folder;
    }

    private static string ReplaceFirst(string text, string oldValue, string newValue)
    {
        var index = text.IndexOf(oldValue, StringComparison.Ordinal);
        return text[..index] + newValue + text[(index + oldValue.Length)..];
    }
}
