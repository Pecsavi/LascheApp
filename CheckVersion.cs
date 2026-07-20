using System.Net.Http.Headers;
using System.Text.Json;

namespace LascheApp;

internal static class CheckVersion
{
    private static readonly HttpClient Http = CreateHttpClient();

    public static async Task<Version?> GetNewerVersionAsync(CancellationToken cancellationToken = default)
    {
        AppSettings settings = AppSettings.Load();
        if (string.IsNullOrWhiteSpace(settings.VersionPath) ||
            !Uri.TryCreate(settings.VersionPath, UriKind.Absolute, out Uri? endpoint))
            return null;

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
            using HttpResponseMessage response = await Http.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LoggerService.Warn($"Version check failed with HTTP {(int)response.StatusCode}.");
                return null;
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            Dictionary<string, ProgramsInfo>? programs = await JsonSerializer.DeserializeAsync<Dictionary<string, ProgramsInfo>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken).ConfigureAwait(false);

            if (programs == null ||
                !programs.TryGetValue(settings.ProgramName, out ProgramsInfo? info) ||
                !Version.TryParse(info.Version, out Version? serverVersion) ||
                !Version.TryParse(LoggerService.GetApplicationVersion(), out Version? localVersion))
                return null;

            return serverVersion > localVersion ? serverVersion : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException or JsonException)
        {
            LoggerService.Warn("Version check failed.", ex);
            return null;
        }
        catch (Exception ex)
        {
            LoggerService.Warn("Unexpected version-check failure.", ex);
            return null;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(8) };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"LascheApp/{LoggerService.GetApplicationVersion()}");
        return client;
    }
}
