using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Repositories;
using IoTPlatform.UnitTests.TestHelpers;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public sealed class TelemetryMetricRepositoryTests
{
    private static DeviceAsset CreateDevice(
        string serialNumber = "IOT-0001-PU") =>
        new(
            SerialNumber.Create(serialNumber),
            "Pump 1",
            DeviceType.Pump,
            "Intake Station",
            new OperatingRange(0, 10, "bar"),
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static TelemetryMetric CreateMetric(
        Guid deviceAssetId,
        MetricType metricType,
        decimal value,
        DateTime recordedAtUtc) =>
        new(
            deviceAssetId,
            metricType,
            value,
            "bar",
            recordedAtUtc);

    [TestMethod]
    public async Task GetForDeviceAsync_TimeWindow_ReturnsOnlyMatchingReadings()
    {
        using var db = SqliteTestDatabase.Create();

        var device = CreateDevice();
        var otherDevice = CreateDevice("IOT-0002-PU");

        db.Context.AddRange(device, otherDevice);
        await db.Context.SaveChangesAsync();

        var fromUtc = new DateTime(
            2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var toUtc = new DateTime(
            2025, 1, 4, 12, 0, 0, DateTimeKind.Utc);

        var beforeWindow = CreateMetric(
            device.Id,
            MetricType.Pressure,
            3m,
            new DateTime(2025, 1, 2, 11, 0, 0, DateTimeKind.Utc));

        var firstInsideWindow = CreateMetric(
            device.Id,
            MetricType.Pressure,
            4m,
            fromUtc);

        var secondInsideWindow = CreateMetric(
            device.Id,
            MetricType.Pressure,
            5m,
            new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc));

        var atWindowEnd = CreateMetric(
            device.Id,
            MetricType.Pressure,
            6m,
            toUtc);

        var afterWindow = CreateMetric(
            device.Id,
            MetricType.Pressure,
            7m,
            new DateTime(2025, 1, 4, 13, 0, 0, DateTimeKind.Utc));

        var otherDeviceMetric = CreateMetric(
            otherDevice.Id,
            MetricType.Pressure,
            8m,
            new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc));

        var repository = new TelemetryMetricRepository(db.Context);

        await repository.AddRangeAsync(
        [
            beforeWindow,
            firstInsideWindow,
            secondInsideWindow,
            atWindowEnd,
            afterWindow,
            otherDeviceMetric
        ]);

        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var results = await new TelemetryMetricRepository(read)
            .GetForDeviceAsync(device.Id, fromUtc, toUtc);

        Assert.HasCount(3, results);
        Assert.AreEqual(firstInsideWindow.Id, results[0].Id);
        Assert.AreEqual(secondInsideWindow.Id, results[1].Id);
        Assert.AreEqual(atWindowEnd.Id, results[2].Id);
    }

    [TestMethod]
    public async Task GetLatestAsync_MultipleReadings_ReturnsNewestMatchingMetric()
    {
        using var db = SqliteTestDatabase.Create();

        var device = CreateDevice();

        db.Context.Add(device);
        await db.Context.SaveChangesAsync();

        var olderPressure = CreateMetric(
            device.Id,
            MetricType.Pressure,
            4m,
            new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        var newestPressure = CreateMetric(
            device.Id,
            MetricType.Pressure,
            8m,
            new DateTime(2025, 1, 4, 12, 0, 0, DateTimeKind.Utc));

        var newerTemperature = CreateMetric(
            device.Id,
            MetricType.Temperature,
            90m,
            new DateTime(2025, 1, 5, 12, 0, 0, DateTimeKind.Utc));

        var repository = new TelemetryMetricRepository(db.Context);

        await repository.AddRangeAsync(
        [
            olderPressure,
            newestPressure,
            newerTemperature
        ]);

        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var result = await new TelemetryMetricRepository(read)
            .GetLatestAsync(device.Id, MetricType.Pressure);

        Assert.IsNotNull(result);
        Assert.AreEqual(newestPressure.Id, result.Id);
        Assert.AreEqual(MetricType.Pressure, result.MetricType);
    }

    [TestMethod]
    public async Task GetLatestAsync_NoMatchingReadings_ReturnsNull()
    {
        using var db = SqliteTestDatabase.Create();

        var device = CreateDevice();

        db.Context.Add(device);
        await db.Context.SaveChangesAsync();

        using var read = db.CreateNewContext();

        var result = await new TelemetryMetricRepository(read)
            .GetLatestAsync(device.Id, MetricType.Pressure);

        Assert.IsNull(result);
    }
}