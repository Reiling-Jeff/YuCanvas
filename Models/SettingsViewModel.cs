using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;
using YuCanvas.Service;

namespace YuCanvas.Models;

public partial class SettingsViewModel : ObservableObject
{
    // --- Konto-Infos (read-only) ---
    [ObservableProperty] private string _accountName = "—";
    [ObservableProperty] private string _accountId = "—";
    [ObservableProperty] private string _accountLocale = "—";

    // --- Einstellungen ---
    [ObservableProperty] private bool _autoSync = true;
    [ObservableProperty] private bool _startOnDashboard = true;

    // --- Rückmeldung an den Nutzer ---
    [ObservableProperty] private string _statusText = "";

    private AppSettings _settings = new();

    public async Task InitAsync()
    {
        _settings = await SettingsService.LoadAsync();
        AutoSync = _settings.AutoSync;
        StartOnDashboard = _settings.StartOnDashboard;
    }

    public void ApplyUser(StudentData user)
    {
        AccountName = user.Name;
        AccountId = user.Id.ToString();
        AccountLocale = user.EffectiveLocale ?? "—";
    }

    [RelayCommand]
    private async Task Save()
    {
        _settings.AutoSync = AutoSync;
        _settings.StartOnDashboard = StartOnDashboard;
        await SettingsService.SaveAsync(_settings);
        StatusText = "Einstellungen gespeichert.";
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        try
        {
            string cacheFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YuCanvas", "courses.json");

            if (File.Exists(cacheFile))
                File.Delete(cacheFile);

            StatusText = "Cache geleert. Beim nächsten Start wird frisch geladen.";
        }
        catch (Exception e)
        {
            StatusText = "Cache konnte nicht geleert werden.";
            Console.WriteLine(e);
        }
        await Task.CompletedTask;
    }
}