using System;
using System.IO;
using System.Text.Json;

namespace TimeTracker.Helpers;

// A record is like a class but designed for holding data.
// The constructor parameters (Width, Height, X, Y) automatically become public properties.
public record WindowSettings(double Width, double Height, double X, double Y);

public static class WindowSettingsService
{
    // Path.Combine builds the path correctly on both macOS (/Users/…) and Windows (C:\Users\…)
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TimeTracker",
        "window.json");

    public static WindowSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<WindowSettings>(File.ReadAllText(FilePath))
                       ?? Default();
        }
        catch { /* corrupt file — fall back to defaults */ }

        return Default();
    }

    public static void Save(WindowSettings settings)
    {
        try
        {
            // CreateDirectory is a no-op when the folder already exists
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings));
        }
        catch { /* ignore write failures (e.g. permissions) */ }
    }

    // double.NaN means "no saved position" — let the OS pick a spot
    private static WindowSettings Default() => new(960, 640, double.NaN, double.NaN);
}
