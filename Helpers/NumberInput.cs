using System.Globalization;

namespace TimeTracker.Helpers;

// Converts between numbers and the text in dialog fields.
// CultureInfo.CurrentCulture respects the user's locale, so "12,50" works on a Dutch Mac.
public static class NumberInput
{
    // Empty input counts as zero; negative numbers are rejected
    public static bool TryParse(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return true;

        return decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value)
               && value >= 0;
    }

    public static bool TryParse(string? text, out double value)
    {
        var isValid = TryParse(text, out decimal decimalValue);
        value = (double)decimalValue;
        return isValid;
    }

    // Zero shows as an empty field; no thousands separator so the text parses back cleanly
    public static string Format(decimal value) =>
        value == 0 ? string.Empty : value.ToString("0.##", CultureInfo.CurrentCulture);

    public static string Format(double value) => Format((decimal)value);
}
