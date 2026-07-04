using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public MainWindowViewModel()
    {
        _currentPage = _dashboard;
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        await LoadFromCacheAsync();
        await LoadFromCanvasAsync();
    }

    private async Task LoadFromCacheAsync()
    {
        List<Course> cached = await CacheService.LoadCoursesAsync();
        _dashboard.ApplyCachedCourses(cached);
    }

    private async Task LoadFromCanvasAsync()
    {
        try
        {
            string baseUrl = Program.Configuration["Canvas:BaseUrl"]!;
            string token   = Program.Configuration["Canvas:Token"]!;

            CanvasService service = new CanvasService(baseUrl, token);
            List<CanvasCourse> canvasCourses = await service.GetCoursesAsync();

            foreach (CanvasCourse c in canvasCourses)
                c.Assignments = await service.GetAssignmentsAsync(c.Id);

            // Beide Seiten mit denselben Daten versorgen
            _dashboard.ApplyCanvasCourses(canvasCourses);
            _assignments.Load(canvasCourses);

            await CacheService.SaveCoursesAsync(_dashboard.Courses.ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _dashboard.MarkSyncFailed();
        }
    }

    [RelayCommand]
    private void ShowDashboard() => CurrentPage = _dashboard;

    [RelayCommand]
    private void ShowAssignments() => CurrentPage = _assignments;
}
