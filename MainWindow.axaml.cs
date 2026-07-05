using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Models;
using MainWindowViewModel = YuCanvas.Models.ViewModels.MainWindowViewModel;

namespace YuCanvas;

public partial class MainWindow : Window
{
    private static MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _mainWindowViewModel;
    }

    public static MainWindowViewModel GetViewModel()
    {
        return _mainWindowViewModel;
    }
    
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
