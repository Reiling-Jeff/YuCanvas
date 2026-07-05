using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;

namespace YuCanvas.Models;

public partial class SideBarViewModel : ObservableObject
{
    [ObservableProperty]
    private string _studentName = "n/a";

    [ObservableProperty]
    private bool _isDashboardActive;

    [ObservableProperty]
    private bool _isAssignmentsActive;

    [ObservableProperty]
    private bool _isSettingsActive;

    public event Action? DashboardRequested;
    public event Action? AssignmentsRequested;
    public event Action? SettingsRequested;

    public void Load(StudentData studentData)
    {
        StudentName = studentData.Name;
    }

    public void SetActivePage(string pageKey)
    {
        IsDashboardActive   = pageKey == "dashboard";
        IsAssignmentsActive = pageKey == "assignments";
        IsSettingsActive    = pageKey == "settings";
    }

    [RelayCommand]
    private void ShowDashboard() => DashboardRequested?.Invoke();

    [RelayCommand]
    private void ShowAssignments() => AssignmentsRequested?.Invoke();

    [RelayCommand]
    private void ShowSettings() => SettingsRequested?.Invoke();
}