using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;
using YuCanvas.Media;
using YuCanvas.Service;

namespace YuCanvas.Models.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowAccount))]
    [NotifyPropertyChangedFor(nameof(ShowBehavior))]
    [NotifyPropertyChangedFor(nameof(ShowData))]
    [NotifyPropertyChangedFor(nameof(ShowConnection))]
    [NotifyPropertyChangedFor(nameof(ShowUpdates))]
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

    [ObservableProperty] private string _currentVersionLabel = $"Version {AppVersion.Current}";
    [ObservableProperty] private string _latestVersionLabel = "";
    [ObservableProperty] private bool _isUpdateAvailable;
    [ObservableProperty] private string _updateStatusText = "Noch nicht geprüft.";
    [ObservableProperty] private string _checkUpdatesButtonText = "Suchen";
    
    [ObservableProperty] private bool _isChangelogOpen;
    [ObservableProperty] private string _changelogText = "";

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
    public bool ShowUpdates    => Matches("update", "aktualisierung", "version", "neu", "release");

    public bool NoResults => !ShowAccount && !ShowBehavior && !ShowData && !ShowConnection && !ShowUpdates;

    public async Task InitAsync()
    {
        _settings = await SettingsService.LoadAsync();
        AutoSync = _settings.AutoSync;
        StartOnDashboard = _settings.StartOnDashboard;
        CanvasBaseUrl = _settings.CanvasBaseUrl;
        CanvasToken = _settings.CanvasToken;

        _ = CheckForUpdatesCommand.ExecuteAsync(null);
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
        StatusText = "Gespeichert.";
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        CheckUpdatesButtonText = "Suche …";
        UpdateStatusText = "Suche nach Updates …";

        UpdateCheckResult result = await UpdateService.CheckAsync();

        CheckUpdatesButtonText = "Suchen";

        if (!result.Success)
        {
            UpdateStatusText = "Prüfung fehlgeschlagen";
            return;
        }

        IsUpdateAvailable = result.IsUpdateAvailable;
        LatestVersionLabel = $"Version {result.LatestVersion}";
        UpdateStatusText = result.IsUpdateAvailable
            ? "Neue Version gefunden."
            : $"Zuletzt geprüft: {DateTime.Now:HH:mm}";
    }

    private bool _changelogLoaded;

    [RelayCommand]
    private async Task OpenChangelog()
    {
        IsChangelogOpen = true;
        if (_changelogLoaded) return;

        ChangelogText = "Lade Änderungen …";
        string? md = await UpdateService.FetchChangelogAsync();

        if (md is null)
        {
            ChangelogText = "Changelog konnte nicht geladen werden. Bist du offline?";
            return;
        }

        ChangelogText = md;
        _changelogLoaded = true;
    }

    [RelayCommand]
    private void CloseChangelog() => IsChangelogOpen = false;

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

            StatusText = "Cache geleert.";
        }
        catch (Exception e)
        {
            StatusText = "Cache konnte nicht geleert werden.";
            Console.WriteLine(e);
        }
    }
}