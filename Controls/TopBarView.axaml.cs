using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YuCanvas.Controls;

public partial class TopBarView : UserControl
{
    public TopBarView() => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
