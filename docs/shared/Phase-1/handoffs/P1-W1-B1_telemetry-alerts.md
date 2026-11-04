# P1-W1-B1 — Telemetry Metrics and Alert Rules

## Owner
Member B — Neel

## Status
Implementation fixes completed. Build and unit tests passing.

## Related Pull Request
- Original PR: #9 — Add TelemetryMetric and AlertRule
- Follow-up fixes PR: Pending

## What's Done
- Added TelemetryMetric domain entity.
- Added AlertRule domain entity.
- Removed optional ID constructor parameters. IDs are generated internally.
- Added AllowedClockSkew constant (5 minutes).
- Added validation for device IDs, metric types, units, alert names, comparison operators, and severity.
- Added alert enable and disable behavior.
- Added telemetry threshold evaluation.
- Added tests for valid and invalid inputs and threshold conditions.

## How to Use

Create a telemetry metric:

```csharp
var metric = new TelemetryMetric(
    deviceAssetId,
    MetricType.Temperature,
    25.5m,
    "Celsius",
    DateTime.UtcNow);
```

Create an alert rule:

```csharp
var rule = new AlertRule(
    deviceAssetId,
    "High Temperature Alert",
    MetricType.Temperature,
    ComparisonOperator.GreaterThan,
    40m,
    AlertSeverity.Warning);
```

Evaluate a metric:

```csharp
bool triggered = rule.IsTriggeredBy(metric);
```

## Important Notes
- IDs are generated internally.
- Telemetry timestamps allow up to five minutes of clock skew.
- Telemetry timestamps are expected to be UTC.
- Equal threshold values do not trigger an alert.
- GreaterThan and LessThan use strict comparisons.
- Invalid values throw DomainValidationException.
- Disabled alert rules never trigger.

## Verification
- `dotnet build` — successful.
- `dotnet test` — 89 passed, 0 failed.

## Next Steps
- Share fixes for review before merging.
- Coordinate with Member A on dependent application work.
- Continue with Member B's next assigned task after review.
