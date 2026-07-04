using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;

namespace YuCanvas.Models;

public partial class SideBarViewModel : ObservableObject
{
    [ObservableProperty]
    private string _studentName = "n/a";

    private static MainWindowViewModel? _mainWindowViewModel;
    private static MainWindowViewModel MainVm => _mainWindowViewModel ??= MainWindow.GetViewModel();

    public void Load(StudentData studentData)
    {
        Console.WriteLine(studentData.Name);
        StudentName = studentData.Name;
    }
    
    
    [RelayCommand]
    private void ShowDashboard() => MainVm.CurrentPage = MainVm.Dashboard;

    [RelayCommand]
    private void ShowAssignments() => MainVm.CurrentPage = MainVm.Assignments;
}