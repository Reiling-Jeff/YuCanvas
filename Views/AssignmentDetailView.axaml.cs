using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using YuCanvas.Media;
using YuCanvas.Models;

namespace YuCanvas.Views;

public partial class AssignmentDetailView : UserControl
{
    public AssignmentDetailView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is not AssignmentDetailViewModel vm)
            return;

        TextBlock? block = this.FindControl<TextBlock>("DescriptionBlock");
        if (block == null)
            return;

        block.Inlines?.Clear();
        foreach (Inline inline in HtmlToInlines.Convert(vm.DescriptionHtml))
            block.Inlines?.Add(inline);
    }
}
