using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Controls;
using YuCanvas.Json;
using YuCanvas.Media;
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

    private AppSettings _appSettings;

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
        
        await LoadFromCacheAsync();
        await LoadFromCanvasAsync();
    }

    private async Task LoadFromCacheAsync()
    {
        
        Console.WriteLine("=== LoadFromCache Start ===");
        List<Course> cachedCourses = await CacheService.LoadCoursesAsync();
        StudentData cachedStudentData = await CacheService.LoadStudentDataAsync();
        _dashboard.ApplyCachedCourses(cachedCourses);
        _assignments.Load(cachedCourses);
        _topBarViewModel.Load(cachedStudentData);
        _sideBarViewModel.Load(cachedStudentData);
        _settings.ApplyUser(cachedStudentData);
        Console.WriteLine("=== LoadFromCache DONE ===");
    }

    private async Task LoadFromCanvasAsync()
    {
        Console.WriteLine("=== LoadFromCanvas START ===");
        try
        {
            string baseUrl = _settings.CanvasBaseUrl;
            string token   = _settings.CanvasToken;

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(token))
            {
                baseUrl = Program.Configuration["Canvas:BaseUrl"] ?? "";
                token   = Program.Configuration["Canvas:Token"] ?? "";

                _settings.CanvasBaseUrl = baseUrl;
                _settings.CanvasToken   = token;
                await SettingsService.SaveAsync(_appSettings);
            }

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(token))
            {
                _dashboard.MarkSyncFailed();
                return;
            }
            Console.WriteLine(1);
            await _settings.InitAsync();
            CanvasService service = new CanvasService(baseUrl, token);
            List<CanvasCourse> canvasCourses = await service.GetCoursesAsync();
            StudentData studentData = await service.GetStudentDataAsync();
            Console.WriteLine(2);
            await Task.WhenAll(canvasCourses.Select(async c =>
            {
                try
                {
                    c.Assignments = await service.GetAssignmentsAsync(c.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kurs {c.Id} fehlgeschlagen: {ex.Message}");
                    c.Assignments = new List<CanvasAssignment>();
                }
            }));
            Console.WriteLine(3);

            _dashboard.ApplyCanvasCourses(canvasCourses);
            _assignments.Load(canvasCourses);
            _topBarViewModel.Load(studentData);
            _sideBarViewModel.Load(studentData);
            _settings.ApplyUser(studentData);
            
            await CacheService.SaveCoursesAsync(_dashboard.Courses.ToList());
            await CacheService.SaveStudentDataAsync(studentData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _dashboard.MarkSyncFailed();
        }
        
        Console.WriteLine("=== LoadFromCanvas END ===");
    }
}
