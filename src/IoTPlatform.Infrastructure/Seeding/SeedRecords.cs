namespace IoTPlatform.Infrastructure.Seeding;

// The shapes of the seed JSON files (docs: SEED_DATA_FORMAT.md §3). Only the loader uses them.
// "required" makes System.Text.Json reject a record with a missing property. Enums stay strings here:
// the loader matches them exactly against the C# names, so a typo is reported with its file and record.

internal sealed record DeviceSeed
{
    public required string SerialNumber { get; init; }
    public required string Name { get; init; }
    public required string DeviceType { get; init; }
    public required string Location { get; init; }
    public required OperatingRangeSeed OperatingRange { get; init; }
    public required DateTime InstalledOnUtc { get; init; }
    public required string Status { get; init; }
}

internal sealed record OperatingRangeSeed
{
    public required decimal Min { get; init; }
    public required decimal Max { get; init; }
    public required string Unit { get; init; }
}

internal sealed record AlertRuleSeed
{
    public required string SerialNumber { get; init; }
    public required string Name { get; init; }
    public required string MetricType { get; init; }
    public required string Operator { get; init; }
    public required decimal Threshold { get; init; }
    public required string Severity { get; init; }
    public required bool IsEnabled { get; init; }
}

internal sealed record MaintenanceLogSeed
{
    public required string SerialNumber { get; init; }
    public required string MaintenanceType { get; init; }
    public required string Technician { get; init; }
    public required string Description { get; init; }
    public required DateTime PerformedOnUtc { get; init; }
}

internal sealed record TelemetrySeed
{
    public required string SerialNumber { get; init; }
    public required string MetricType { get; init; }
    public required decimal Value { get; init; }
    public required string Unit { get; init; }
    public required DateTime RecordedAtUtc { get; init; }
}
