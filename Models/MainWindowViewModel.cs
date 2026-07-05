using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using YuCanvas.Controls;
using YuCanvas.Json;
using YuCanvas.Service;

namespace YuCanvas.Models;

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
        _ = InitAsync();
    }

    private void ShowAssignmentDetail(CanvasAssignment assignment)
    {
        AssignmentDetailViewModel detail = new AssignmentDetailViewModel(assignment);
        detail.BackRequested += () => CurrentPage = _assignments;
        CurrentPage = detail;
    }

    private async Task InitAsync()
    {
        _appSettings = await SettingsService.LoadAsync();
        await _settings.InitAsync();

        SyncResult cached = await _syncService.LoadFromCacheAsync();
        ApplyResult(cached, isFromCache: true);

        if (!_settings.AutoSync)
        {
            Dashboard.LastSyncText = "Automatisches Synchronisieren ist ausgeschaltet.";
            Dashboard.PassedSync = false; 
            return;
        }

        SyncResult synced = await _syncService.SyncFromCanvasAsync(_appSettings);
        if (synced.Success)
        {
            ApplyResult(synced, isFromCache: false);
        }
        else
        {
            _dashboard.MarkSyncFailed();
        }
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
            _dashboard.ApplyCanvasCourses(result.CanvasCourses);
            _assignments.Load(result.CanvasCourses);
        }

        if (result.StudentData == null) return;
        
        _topBarViewModel.Load(result.StudentData);
        _sideBarViewModel.Load(result.StudentData);
        _settings.ApplyUser(result.StudentData);
    }
}