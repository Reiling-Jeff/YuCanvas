using YuCanvas.Json;

namespace YuCanvas.Media;

public class Assignment
{
    public string Name { get; set; } = "";

    public CanvasSubmission? Submission { get; set; }
}
