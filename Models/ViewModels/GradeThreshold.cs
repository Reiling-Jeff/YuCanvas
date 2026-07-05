using CommunityToolkit.Mvvm.ComponentModel;

namespace YuCanvas.Models.ViewModels;

public partial class GradeThresholdEntry : ObservableObject
{
    [ObservableProperty]
    private string _label = "";

    [ObservableProperty]
    private string _minPoints = "";

    [ObservableProperty]
    private string _minPercent = "";
}