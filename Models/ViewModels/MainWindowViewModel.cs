using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using YuCanvas.Controls;
using YuCanvas.Json;
using YuCanvas.Media;
using YuCanvas.Service;

namespace YuCanvas.Models.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object _currentPage;

    private readonly DashboardViewModel _dashboard = new();
    private readonly AssignmentsViewModel _assignments = new();
    private readonly TopBarViewModel _topBarViewModel = TopBarView.GetViewModel();
    private readonly SideBarViewModel _sideBarViewModel = SidebarView.GetViewModel();
    private readonly SettingsViewModel _settings = new();
    private readonly SyncService _syncService = new();

    private AppSettings _appSettings = new();

    public DashboardViewModel Dashboard => _dashboard;
    public AssignmentsViewModel Assignments => _assignments;
    public SettingsViewModel Settings => _settings;

    public MainWindowViewModel()
    {
        _currentPage = _dashboard;

        _assignments.AssignmentSelected += ShowAssignmentDetail;
        _dashboard.AssignmentSelected += ShowAssignmentDetail;

        _sideBarViewModel.DashboardRequested   += () => SetPage(_dashboard, "dashboard");
        _sideBarViewModel.AssignmentsRequested += () => SetPage(_assignments, "assignments");
        _sideBarViewModel.SettingsRequested    += () => SetPage(_settings, "settings");
        _topBarViewModel.SyncRequested         += () => _ = SyncAsync();

        SetPage(_dashboard, "dashboard");

        _ = InitAsync();
    }

    private void SetPage(object page, string pageKey)
    {
        CurrentPage = page;
        _sideBarViewModel.SetActivePage(pageKey);
    }

    private void ShowAssignmentDetail(CanvasAssignment assignment)
    {
        var detail = new ViewModels.AssignmentDetailViewModel(assignment);
        detail.BackRequested += () => SetPage(_assignments, "assignments");
        CurrentPage = detail;
    }

    private async Task SyncAsync()
    {
        _dashboard.LastSyncText = "Synchronisiert...";
        _dashboard.PassedSync = false;
        SyncResult synced = await _syncService.SyncFromCanvasAsync(_appSettings);
        if (synced.Success)
            ApplyResult(synced, false);
        else
            _dashboard.MarkSyncFailed();
    }
    
    private async Task InitAsync()
    {
        await _settings.InitAsync();
        _appSettings = SettingsViewModel.Settings;

        SyncResult cached = await _syncService.LoadFromCacheAsync();
        ApplyResult(cached, isFromCache: true);

        if (!_settings.AutoSync)
        {
            _dashboard.LastSyncText = "Automatisches Synchronisieren ist ausgeschaltet.";
            _dashboard.PassedSync = false; 
            return;
        }

        await SyncAsync();
    }

    private void ApplyResult(SyncResult result, bool isFromCache)
    {
        if (isFromCache)
        {
            _dashboard.ApplyCachedCourses(result.Courses);
            _assignments.Load(result.Courses);
        }
        else
        {
            _dashboard.ApplyCanvasCourses(result.Courses);
            _assignments.Load(result.Courses);
        }

        if (result.StudentData == null) return;
        
        _topBarViewModel.Load(result.StudentData);
        _sideBarViewModel.Load(result.StudentData);
        _settings.ApplyUser(result.StudentData);
    }
}