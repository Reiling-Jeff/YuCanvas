using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;
using YuCanvas.Media;
using YuCanvas.Service;

namespace YuCanvas.Models;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowAccount))]
    [NotifyPropertyChangedFor(nameof(ShowBehavior))]
    [NotifyPropertyChangedFor(nameof(ShowData))]
    [NotifyPropertyChangedFor(nameof(ShowConnection))]
    [NotifyPropertyChangedFor(nameof(NoResults))]
    private string _searchQuery = "";

    [ObservableProperty] private string _accountName = "—";
    [ObservableProperty] private string _accountId = "—";
    [ObservableProperty] private string _accountLocale = "—";

    [ObservableProperty] private bool _autoSync = true;
    [ObservableProperty] private bool _startOnDashboard = true;

    [ObservableProperty] private string _canvasBaseUrl = "";
    [ObservableProperty] private string _canvasToken = "";

    [ObservableProperty] private string _statusText = "";

    private AppSettings _settings = new();

    private bool Matches(params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return true;

        string q = SearchQuery.Trim().ToLowerInvariant();
        foreach (string kw in keywords)
        {
            if (kw.ToLowerInvariant().Contains(q))
                return true;
        }
        return false;
    }

    public bool ShowAccount    => Matches("konto", "account", "name", "nutzer", "id", "sprache", "profil");
    public bool ShowBehavior   => Matches("verhalten", "sync", "synchronisieren", "automatisch", "dashboard", "start");
    public bool ShowData       => Matches("daten", "cache", "zwischenspeicher", "leeren", "löschen");
    public bool ShowConnection => Matches("canvas", "verbindung", "url", "token", "zugriff", "anmeldung");

    public bool NoResults => !ShowAccount && !ShowBehavior && !ShowData && !ShowConnection;

    public async Task InitAsync()
    {
        _settings = await SettingsService.LoadAsync();
        AutoSync = _settings.AutoSync;
        StartOnDashboard = _settings.StartOnDashboard;
        CanvasBaseUrl = _settings.CanvasBaseUrl;
        CanvasToken = _settings.CanvasToken;
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
        _settings.CanvasBaseUrl = CanvasBaseUrl.Trim();
        _settings.CanvasToken = CanvasToken.Trim();
        await SettingsService.SaveAsync(_settings);
        StatusText = "Gespeichert. Zum Übernehmen neu synchronisieren.";
    }

    [RelayCommand]
    private void ClearCache()
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
    }
}