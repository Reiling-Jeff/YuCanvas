namespace YuCanvas.Media;

public class AppSettings
{
    public bool AutoSync { get; set; } = true;
    public bool StartOnDashboard { get; set; } = true;
    public string CanvasBaseUrl { get; set; } = "";
    public string CanvasToken { get; set; } = "";

    public string ThemeSchemeId { get; set; } = "amber";
    public string CustomAccentHex { get; set; } = "#E2A63C";
    public string CustomThemeName { get; set; } = "Eigenes Schema";
}