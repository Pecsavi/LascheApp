using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

namespace LascheApp;

internal static class LoggerService
{
    private static readonly object FileLock = new();
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LascheApp", "Logs");
    private static readonly string LogPath = Path.Combine(LogDirectory, "LascheApp.log");
    private static AppSettings? _settings;

    public static void Initialize()
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            _settings = AppSettings.Load();
            Info("Application logging initialized.");
        }
        catch
        {
            // Logging must never prevent the application from starting.
        }
    }

    public static void Info(string message) => Write("INFO", message, null);
    public static void Warn(string message, Exception? exception = null) => Write("WARN", message, exception);
    public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    public static async Task TrackEventAsync(string eventName)
    {
        AppSettings settings = _settings ??= AppSettings.Load();
        if (!settings.TelemetryEnabled ||
            string.IsNullOrWhiteSpace(settings.RemoteLogUrl) ||
            !Uri.TryCreate(settings.RemoteLogUrl, UriKind.Absolute, out Uri? endpoint))
            return;

        // Keep this payload deliberately small. Never add usernames, project data,
        // report contents, document names or file paths here.
        var payload = new
        {
            program = settings.ProgramName,
            @event = eventName,
            machine = Environment.MachineName,
            timestamp = DateTimeOffset.Now,
            version = GetApplicationVersion()
        };

        try
        {
            using HttpResponseMessage response = await Http.PostAsJsonAsync(endpoint, payload)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                Warn($"Telemetry request failed with HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            Warn("Telemetry request failed.", ex);
        }
        catch (Exception ex)
        {
            Warn("Unexpected telemetry failure.", ex);
        }
    }

    public static string GetApplicationVersion()
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                .ProductVersion?.Split('+')[0] ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            string exceptionText = exception == null
                ? string.Empty
                : $" | {exception.GetType().Name}: {exception.Message}";
            string line = $"{DateTimeOffset.Now:O} | {level} | {message}{exceptionText}{Environment.NewLine}";
            lock (FileLock)
                File.AppendAllText(LogPath, line);
        }
        catch
        {
            // Logging failures are non-fatal by design.
        }
    }
}
