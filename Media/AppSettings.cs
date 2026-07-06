using System.Collections.Generic;
using YuCanvas.Models.ViewModels;

namespace YuCanvas.Media;

public class AppSettings
{
    public bool AutoSync { get; set; } = true;
    public string CanvasBaseUrl { get; set; } = "";
    public string CanvasToken { get; set; } = "";
    public List<GradeThresholdEntry>? GradeThresholdEntries { get; set; }
    public List<long> CollectionDeadlineEntries { get; set; } = new();
}