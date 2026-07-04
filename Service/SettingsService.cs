using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using YuCanvas.Models;

namespace YuCanvas.Service;

public static class SettingsService
{
    private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YuCanvas",
        "settings.json");

    public static async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(SettingsFile))
            return new AppSettings();

        try
        {
            string json = await File.ReadAllTextAsync(SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
        string json = JsonSerializer.Serialize(settings,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(SettingsFile, json);
    }
}