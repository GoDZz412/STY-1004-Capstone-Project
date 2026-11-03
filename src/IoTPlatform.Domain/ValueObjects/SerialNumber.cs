using System.Text.RegularExpressions;
using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.Domain.ValueObjects;

/// <summary>
/// Device serial number. Format mask <c>IOT-####-AA</c>: literal "IOT-", 4 digits, "-", 2 uppercase letters.
/// Input is trimmed and upper-cased before validation. Example: <c>IOT-0042-PX</c>.
/// </summary>
public sealed record SerialNumber
{
    public const string Pattern = @"^IOT-\d{4}-[A-Z]{2}$";
    public const int MaxLength = 11;

    // ECMAScript option keeps \d ASCII-only (otherwise .NET also accepts Arabic-Indic and other Unicode digits).
    private static readonly Regex Mask = new(Pattern, RegexOptions.Compiled | RegexOptions.ECMAScript);

    private SerialNumber(string value) => Value = value;

    public string Value { get; }

    /// <exception cref="InvalidSerialNumberException">Null, empty, whitespace, or not matching the mask.</exception>
    public static SerialNumber Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new InvalidSerialNumberException(input);

        var normalized = input.Trim().ToUpperInvariant();
        if (!Mask.IsMatch(normalized))
            throw new InvalidSerialNumberException(input);

        return new SerialNumber(normalized);
    }

    public static bool TryCreate(string? input, out SerialNumber? serialNumber)
    {
        try
        {
            serialNumber = Create(input);
            return true;
        }
        catch (InvalidSerialNumberException)
        {
            serialNumber = null;
            return false;
        }
    }

    public override string ToString() => Value;
}