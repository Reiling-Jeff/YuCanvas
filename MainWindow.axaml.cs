using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Models;

namespace YuCanvas;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
