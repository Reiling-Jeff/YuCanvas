using Avalonia.Media;
using YuCanvas.Json;

namespace YuCanvas.Media;

public class Deadline
{
    public string Title { get; set; } = "";
    public string Course { get; set; } = "";
    public string DueLabel { get; set; } = "";
    public string Relative { get; set; } = "";
    public IBrush AccentColor { get; set; } = Brushes.MediumPurple;
    public CanvasAssignment? Source { get; set; }
}
