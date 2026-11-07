using System.Text.Json;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Seeding;

/// <summary>
/// Loads the sample data in <c>Seeding/Data/*.json</c> into an empty database. Every object is built through the domain
/// constructors and methods, so the seed data obeys the same rules as real data. Runs on <c>dotnet ef database update</c>
/// (see <see cref="DesignTimeDbContextFactory"/>). Does nothing when the database already has devices.
/// </summary>
public sealed class JsonSeedLoader
{
    public const string DevicesFile = "devices.json";
    public const string AlertRulesFile = "alert-rules.json";
    public const string MaintenanceLogsFile = "maintenance-logs.json";
    public const string TelemetryFile = "telemetry.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly string _dataDirectory;

    public JsonSeedLoader()
        : this(DefaultDataDirectory)
    {
    }

    public JsonSeedLoader(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        _dataDirectory = dataDirectory;
    }

    /// <summary>
    /// <c>Seeding/Data</c> next to <c>IoTPlatform.Infrastructure.dll</c> (the .csproj copies the files there). Not
    /// <c>AppContext.BaseDirectory</c>: under <c>dotnet ef</c> that is the EF tool's own folder.
    /// </summary>
    public static string DefaultDataDirectory { get; } = Path.Combine(
        Path.GetDirectoryName(typeof(JsonSeedLoader).Assembly.Location) is { Length: > 0 } folder ? folder : AppContext.BaseDirectory,
        "Seeding", "Data");

    public async Task SeedAsync(IoTPlatformDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (await context.DeviceAssets.AnyAsync(cancellationToken))
        {
            return;
        }

        AddSeedData(context);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Synchronous version for EF Core's <c>UseSeeding</c>.</summary>
    public void Seed(IoTPlatformDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.DeviceAssets.Any())
        {
            return;
        }

        AddSeedData(context);
        context.SaveChanges();
    }

    // Builds everything in memory first and only then adds it to the context, so a bad record leaves nothing behind.
    private void AddSeedData(IoTPlatformDbContext context)
    {
        var deviceRecords = Read<DeviceSeed>(DevicesFile);
        var devices = BuildDevices(deviceRecords);
        AddAlertRules(devices, Read<AlertRuleSeed>(AlertRulesFile));
        AddMaintenanceLogs(devices, Read<MaintenanceLogSeed>(MaintenanceLogsFile));
        var telemetry = BuildTelemetry(devices, Read<TelemetrySeed>(TelemetryFile));

        // Status last: rules and history can still be added to a device that ends up Decommissioned.
        for (var i = 0; i < deviceRecords.Count; i++)
        {
            var record = deviceRecords[i];
            Run(DevicesFile, i, record.SerialNumber,
                () => MoveToStatus(devices[record.SerialNumber], ParseEnum<DeviceStatus>(record.Status, "status")));
        }

        context.DeviceAssets.AddRange(devices.Values);
        context.TelemetryMetrics.AddRange(telemetry);
    }

    private static Dictionary<string, DeviceAsset> BuildDevices(IReadOnlyList<DeviceSeed> records)
    {
        var devices = new Dictionary<string, DeviceAsset>(StringComparer.Ordinal);
        for (var i = 0; i < records.Count; i++)
        {
            var r = records[i];
            Run(DevicesFile, i, r.SerialNumber, () =>
            {
                if (r.OperatingRange is null)
                {
                    throw new FormatException("operatingRange is missing.");
                }

                var device = new DeviceAsset(
                    SerialNumber.Create(r.SerialNumber),
                    r.Name,
                    ParseEnum<DeviceType>(r.DeviceType, "deviceType"),
                    r.Location,
                    new OperatingRange(r.OperatingRange.Min, r.OperatingRange.Max, r.OperatingRange.Unit),
                    r.InstalledOnUtc);

                if (!devices.TryAdd(device.SerialNumber.Value, device))
                {
                    throw new FormatException($"serial number {device.SerialNumber} appears twice.");
                }
            });
        }

        return devices;
    }

    private static void AddAlertRules(Dictionary<string, DeviceAsset> devices, IReadOnlyList<AlertRuleSeed> records)
    {
        for (var i = 0; i < records.Count; i++)
        {
            var r = records[i];
            Run(AlertRulesFile, i, r.SerialNumber, () =>
            {
                var rule = FindDevice(devices, r.SerialNumber).AddAlertRule(
                    r.Name,
                    ParseEnum<MetricType>(r.MetricType, "metricType"),
                    ParseEnum<ComparisonOperator>(r.Operator, "operator"),
                    r.Threshold,
                    ParseEnum<AlertSeverity>(r.Severity, "severity"));

                if (!r.IsEnabled)
                {
                    rule.Disable();
                }
            });
        }
    }

    private static void AddMaintenanceLogs(Dictionary<string, DeviceAsset> devices, IReadOnlyList<MaintenanceLogSeed> records)
    {
        for (var i = 0; i < records.Count; i++)
        {
            var r = records[i];
            Run(MaintenanceLogsFile, i, r.SerialNumber, () =>
                FindDevice(devices, r.SerialNumber).LogMaintenance(
                    ParseEnum<MaintenanceType>(r.MaintenanceType, "maintenanceType"),
                    r.Technician,
                    r.Description,
                    r.PerformedOnUtc));
        }
    }

    private static List<TelemetryMetric> BuildTelemetry(Dictionary<string, DeviceAsset> devices, IReadOnlyList<TelemetrySeed> records)
    {
        var metrics = new List<TelemetryMetric>(records.Count);
        for (var i = 0; i < records.Count; i++)
        {
            var r = records[i];
            Run(TelemetryFile, i, r.SerialNumber, () =>
                metrics.Add(new TelemetryMetric(
                    FindDevice(devices, r.SerialNumber).Id,
                    ParseEnum<MetricType>(r.MetricType, "metricType"),
                    r.Value,
                    r.Unit,
                    r.RecordedAtUtc)));
        }

        return metrics;
    }

    // Only the allowed lifecycle steps (PHASE_1 §3.4), starting from Provisioned.
    private static void MoveToStatus(DeviceAsset device, DeviceStatus target)
    {
        switch (target)
        {
            case DeviceStatus.Provisioned:
                break;
            case DeviceStatus.Active:
                device.Activate();
                break;
            case DeviceStatus.UnderMaintenance:
                device.Activate();
                device.StartMaintenance();
                break;
            case DeviceStatus.Decommissioned:
                device.Decommission();
                break;
        }
    }

    private static DeviceAsset FindDevice(Dictionary<string, DeviceAsset> devices, string? serialNumber) =>
        serialNumber is not null && devices.TryGetValue(serialNumber, out var device)
            ? device
            : throw new FormatException($"serial number '{serialNumber}' is not in {DevicesFile}.");

    // Exact, case-sensitive match on the C# name: "pump" or "1" are rejected.
    private static TEnum ParseEnum<TEnum>(string? value, string property) where TEnum : struct, Enum =>
        value is not null && Enum.GetNames<TEnum>().Contains(value, StringComparer.Ordinal)
            ? Enum.Parse<TEnum>(value)
            : throw new FormatException(
                $"{property} '{value}' is not one of: {string.Join(", ", Enum.GetNames<TEnum>())}.");

    // Turns a broken record into one clear error: file, record number (0-based), serial and the reason.
    private static void Run(string fileName, int index, string? serialNumber, Action build)
    {
        try
        {
            build();
        }
        catch (Exception ex) when (ex is DomainException or FormatException)
        {
            throw new InvalidOperationException(
                $"Seed data error in {fileName}, record {index} ({serialNumber ?? "no serialNumber"}): {ex.Message}", ex);
        }
    }

    private IReadOnlyList<T> Read<T>(string fileName)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        try
        {
            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException($"Seed data error in {fileName}: the file is empty (record list expected).");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Seed data error in {fileName}, record at {ex.Path}: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Seed data error: cannot read {fileName} from '{_dataDirectory}'.", ex);
        }
    }
}
