using System.Collections.Generic;
using Avalonia.Media;
using YuCanvas.Json;

namespace YuCanvas.Media;

public class Course
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public List<CanvasTeacher> Teachers { get; set; }
    public int Progress { get; set; }
    
    public List<CanvasAssignment>? Assignments { get; set; }
}
