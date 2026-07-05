using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YuCanvas.Models;

namespace YuCanvas.Views.Components;

public partial class Badge : UserControl
{
    private BadgeModel _badgeModel = new BadgeModel();
    
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Label), "Beta");
    
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    
    public Badge()
    {
        InitializeComponent();
        DataContext = _badgeModel;
    }
    
}
