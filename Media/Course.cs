using System.Collections.Generic;
using Avalonia.Media;
using YuCanvas.Json;

namespace YuCanvas.Media;

public class Course
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Lecturer { get; set; } = "";
    public int Progress { get; set; }
    
    public List<CanvasAssignment>? Assignments { get; set; }
}
