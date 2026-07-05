using CommunityToolkit.Mvvm.ComponentModel;

namespace YuCanvas.Models;

public partial class BadgeModel : ObservableObject
{
    [ObservableProperty]
    private string _text = "";
}