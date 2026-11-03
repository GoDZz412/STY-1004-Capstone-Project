using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.Domain.ValueObjects;

/// <summary>Safe operating limits of a device: <c>Min &gt;= 0</c>, <c>Max &gt; Min</c>, with a unit (max 20 chars).</summary>
public sealed record OperatingRange
{
    public const int UnitMaxLength = 20;

    public OperatingRange(decimal min, decimal max, string unit)
    {
        if (min < 0)
            throw new InvalidOperatingRangeException($"Minimum ({min}) must be zero or greater.");
        if (max <= min)
            throw new InvalidOperatingRangeException($"Maximum ({max}) must be greater than minimum ({min}).");
        if (string.IsNullOrWhiteSpace(unit))
            throw new InvalidOperatingRangeException("Unit is required.");

        unit = unit.Trim();
        if (unit.Length > UnitMaxLength)
            throw new InvalidOperatingRangeException($"Unit must be at most {UnitMaxLength} characters.");

        Min = min;
        Max = max;
        Unit = unit;
    }

    public decimal Min { get; }
    public decimal Max { get; }
    public string Unit { get; }

    /// <summary>True when <paramref name="value"/> lies within the range, inclusive on both ends.</summary>
    public bool Contains(decimal value) => value >= Min && value <= Max;
}