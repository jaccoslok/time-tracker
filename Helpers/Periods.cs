using System;
using System.Collections.Generic;

namespace TimeTracker.Helpers;

// The fixed periods in the "Period" list, and the date range each one covers
public static class Periods
{
    public static IReadOnlyList<string> Names { get; } =
        ["Today", "Yesterday", "This Week", "Last Week", "This Month", "Last Month", "This Year", "Last Year", "All Time"];

    public const int ThisWeek = 2;

    // Start (inclusive) and end (exclusive) of a period in local time. Weeks start on Monday.
    // Returns a tuple: the caller can write  var (from, to) = Periods.GetRange(index);
    public static (DateTime From, DateTime To) GetRange(int index)
    {
        var today = DateTime.Today;
        // DayOfWeek counts Sunday = 0 … Saturday = 6; "+ 6) % 7" makes Monday = 0
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var firstOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var firstOfYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);

        // A switch expression: like a CASE statement that returns a value
        return index switch
        {
            0 => (today, today.AddDays(1)),
            1 => (today.AddDays(-1), today),
            2 => (monday, monday.AddDays(7)),
            3 => (monday.AddDays(-7), monday),
            4 => (firstOfMonth, firstOfMonth.AddMonths(1)),
            5 => (firstOfMonth.AddMonths(-1), firstOfMonth),
            6 => (firstOfYear, firstOfYear.AddYears(1)),
            7 => (firstOfYear.AddYears(-1), firstOfYear),
            _ => (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local),
                  new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Local))
        };
    }
}
