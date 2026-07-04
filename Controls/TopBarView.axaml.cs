using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Models;

namespace YuCanvas.Controls;

public partial class TopBarView : UserControl
{
    private static TopBarViewModel _viewModel = new TopBarViewModel();
    
    public TopBarView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public static TopBarViewModel GetViewModel()
    {
        return _viewModel;
    }
}
