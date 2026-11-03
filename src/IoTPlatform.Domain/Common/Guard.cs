using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.Domain.Common;

/// <summary>Small validation helpers shared by all domain types. Throws <see cref="DomainValidationException"/>.</summary>
internal static class Guard
{
    /// <summary>Required text, trimmed, 1..maxLength characters. Returns the trimmed value.</summary>
    public static string RequiredText(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{field} is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainValidationException($"{field} must be at most {maxLength} characters.");

        return trimmed;
    }

    public static Guid NotEmpty(Guid value, string field)
    {
        if (value == Guid.Empty)
            throw new DomainValidationException($"{field} cannot be an empty Guid.");
        return value;
    }

    /// <summary>The value must not be later than now (UTC) plus an optional tolerance (clock skew).</summary>
    public static DateTime NotInFuture(DateTime valueUtc, string field, TimeSpan? tolerance = null)
    {
        if (valueUtc > DateTime.UtcNow + (tolerance ?? TimeSpan.Zero))
            throw new DomainValidationException($"{field} cannot be in the future.");
        return valueUtc;
    }

    public static TEnum Defined<TEnum>(TEnum value, string field) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainValidationException($"{field} has an invalid value '{value}'.");
        return value;
    }

    public static T Required<T>(T? value, string field) where T : class =>
        value ?? throw new DomainValidationException($"{field} is required.");
}