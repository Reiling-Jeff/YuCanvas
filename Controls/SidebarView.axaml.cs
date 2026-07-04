using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YuCanvas.Controls;

public partial class SidebarView : UserControl
{
    public SidebarView() => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
