using System;
using System.Net.Http;
using System.Threading.Tasks;
using YuCanvas.Media;

namespace YuCanvas.Service;

public class UpdateCheckResult
{
    public bool Success { get; init; }
    public string LatestVersion { get; init; } = "";
    public bool IsUpdateAvailable { get; init; }
}

public static class UpdateService
{
    private const string VersionUrl = "https://raw.githubusercontent.com/Reiling-Jeff/YuCanvas/main/VERSION";
    private const string ChangelogUrl = "https://raw.githubusercontent.com/Reiling-Jeff/YuCanvas/main/CHANGELOG.md";

    public static async Task<UpdateCheckResult> CheckAsync()
    {
        try
        {
            using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(8) };
            string remote = (await http.GetStringAsync(VersionUrl)).Trim();

            if (string.IsNullOrWhiteSpace(remote))
                return new UpdateCheckResult { Success = false };

            return new UpdateCheckResult
            {
                Success = true,
                LatestVersion = remote,
                IsUpdateAvailable = !string.Equals(remote, AppVersion.Current, StringComparison.OrdinalIgnoreCase)
            };
        }
        catch
        {
            return new UpdateCheckResult { Success = false };
        }
    }

    public static async Task<string?> FetchChangelogAsync()
    {
        try
        {
            using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(8) };
            string text = await http.GetStringAsync(ChangelogUrl);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }
}