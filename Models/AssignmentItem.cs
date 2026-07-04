using Avalonia.Media;
using YuCanvas.Json;

namespace YuCanvas.Models;

public class AssignmentItem
{
    public string Name { get; set; } = "";
    public string CourseName { get; set; } = "";
    public string StatusText { get; set; } = "";
    public IBrush StatusColor { get; set; } = Brushes.Gray;
    public CanvasAssignment Source { get; set; } = null!;
}
