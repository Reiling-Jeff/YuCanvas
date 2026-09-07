using Avalonia.Media;

namespace YuCanvas.Models;

public class ColorScheme
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    public required Color Accent { get; init; }
    public required Color AccentBright { get; init; }
    public required Color AccentDeep { get; init; }
    public required Color InkOnAccent { get; init; }

    public required Color BgDeep { get; init; }
    public required Color BgBase { get; init; }
    public required Color BgRaised { get; init; }
    public required Color BgRaisedHover { get; init; }
    public required Color BgSidebar { get; init; }
    public required Color BgInset { get; init; }

    public required Color Border { get; init; }
    public required Color BorderBright { get; init; }
    public required Color BorderStrong { get; init; }

    public required Color TextPrimary { get; init; }
    public required Color TextSecondary { get; init; }
    public required Color TextMuted { get; init; }

    public required Color Success { get; init; }
    public required Color Warn { get; init; }
    public required Color Danger { get; init; }
    public required Color Info { get; init; }
}
