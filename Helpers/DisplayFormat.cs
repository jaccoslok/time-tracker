using System;
using System.Globalization;

namespace TimeTracker.Helpers;

// Turns durations and amounts into the text shown in lists, cards and charts
public static class DisplayFormat
{
    // Becomes a user setting in the Settings window phase
    public static string CurrencySymbol { get; set; } = "€";

    // h:mm rounded to the nearest minute, e.g. 1h 45m → "1:45"
    public static string Duration(TimeSpan duration)
    {
        var totalMinutes = Math.Max(0, (int)Math.Round(duration.TotalMinutes));
        return $"{totalMinutes / 60}:{totalMinutes % 60:D2}";
    }

    public static string Hours(double hours) => Duration(TimeSpan.FromHours(hours));

    // h:mm:ss for the running timer
    public static string Clock(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        return $"{(int)elapsed.TotalHours}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    // "€ 1.234,50" — grouping and decimal separator follow the user's locale
    public static string Money(decimal amount)
    {
        var text = amount.ToString("#,##0.00", CultureInfo.CurrentCulture);
        return string.IsNullOrEmpty(CurrencySymbol) ? text : $"{CurrencySymbol} {text}";
    }
}
