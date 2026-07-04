using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Media;
using YuCanvas.Models;
using YuCanvas.Service;

namespace YuCanvas.Views;

public partial class DashboardView : UserControl
{
    private DashboardViewModel _vm;
    
    public DashboardView()
    {
        InitializeComponent();
        _vm = new DashboardViewModel();
        DataContext = _vm;
        _ = InitAsync(_vm);
    }
    
    private async Task InitAsync(DashboardViewModel vm)
    {
        await vm.LoadFromCacheAsync();
        await vm.LoadFromCanvasAsync();
    }
    
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
