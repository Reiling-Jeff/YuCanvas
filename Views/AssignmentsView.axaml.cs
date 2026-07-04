using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YuCanvas.Views;

public partial class AssignmentsView : UserControl
{
    public AssignmentsView() => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
