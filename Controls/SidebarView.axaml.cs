using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Models;

namespace YuCanvas.Controls;

public partial class SidebarView : UserControl
{
    private static SideBarViewModel _sideBarViewModel = new SideBarViewModel();
    
    public SidebarView()
    {
        InitializeComponent();
        DataContext = _sideBarViewModel;
    }

    public static SideBarViewModel GetViewModel()
    {
        return _sideBarViewModel;
    }
    
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
