using System.Text.Json;

namespace LascheApp;

internal sealed class AppSettings
{
    public string ProgramName { get; init; } = "LascheApp";
    public string? VersionPath { get; init; }
    public string? RemoteLogUrl { get; init; }
    public bool TelemetryEnabled { get; init; }

    public static AppSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(path))
            return new AppSettings();

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            LoggerService.Warn("Configuration could not be loaded; network features are disabled.", ex);
            return new AppSettings();
        }
    }
}
