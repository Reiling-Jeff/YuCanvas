namespace YuCanvas.Models;

public class AppSettings
{
    public bool AutoSync { get; set; } = true;
    public bool StartOnDashboard { get; set; } = true;
    public string CanvasBaseUrl { get; set; } = "";
    public string CanvasToken { get; set; } = "";
}