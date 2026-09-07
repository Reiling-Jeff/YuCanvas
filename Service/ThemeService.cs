using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using YuCanvas.Media;
using YuCanvas.Models;

namespace YuCanvas.Service;

/// <summary>
/// Every scheme (preset or user-made) is derived from a single accent colour,
/// so picking your own accent always produces a full, coherent "ledger" palette
/// instead of asking for twenty individual colour choices.
/// </summary>
public static class ThemeService
{
    private static readonly (string Id, string Name, string Hex)[] PresetSeeds =
    {
        ("amber",  "Bernstein", "#E2A63C"),
        ("copper", "Kupfer",    "#C9723C"),
        ("sage",   "Salbei",    "#7A9B6E"),
        ("ink",    "Tinte",     "#5C86A6"),
        ("cherry", "Kirsch",    "#B4485A"),
    };

    public static IReadOnlyList<ColorScheme> Presets { get; } =
        PresetSeeds.Select(s => Generate(s.Id, s.Name, Color.Parse(s.Hex))).ToList();

    public static ColorScheme Default => Presets[0];

    public static ColorScheme FindPreset(string id) =>
        Presets.FirstOrDefault(p => p.Id == id) ?? Default;

    public static ColorScheme Resolve(AppSettings settings)
    {
        if (settings.ThemeSchemeId == "custom" && Color.TryParse(settings.CustomAccentHex, out Color accent))
        {
            string name = string.IsNullOrWhiteSpace(settings.CustomThemeName) ? "Eigenes Schema" : settings.CustomThemeName;
            return Generate("custom", name, accent);
        }
        return FindPreset(settings.ThemeSchemeId);
    }

    public static ColorScheme Generate(string id, string name, Color accent)
    {
        double hue = accent.ToHsl().H;

        Color Surface(double lightness) => new HslColor(1, hue, 0.10, lightness).ToRgb();
        Color Ink(double lightness) => new HslColor(1, hue, 0.06, lightness).ToRgb();

        Color ink = RelativeLuminance(accent) > 0.5 ? Color.Parse("#171310") : Color.Parse("#F1ECE1");

        return new ColorScheme
        {
            Id = id,
            Name = name,
            Accent = accent,
            AccentBright = Lighten(accent, 0.12),
            AccentDeep = Lighten(accent, -0.16),
            InkOnAccent = ink,

            BgDeep = Surface(0.070),
            BgBase = Surface(0.086),
            BgRaised = Surface(0.106),
            BgRaisedHover = Surface(0.130),
            BgSidebar = Surface(0.055),
            BgInset = Surface(0.045),

            Border = Surface(0.165),
            BorderBright = Surface(0.245),
            BorderStrong = Surface(0.335),

            TextPrimary = Ink(0.94),
            TextSecondary = Ink(0.68),
            TextMuted = Ink(0.45),

            Success = Color.Parse("#6FA35B"),
            Warn = Color.Parse("#CC7A2E"),
            Danger = Color.Parse("#C1483C"),
            Info = Color.Parse("#4E8B86"),
        };
    }

    public static void Apply(ColorScheme scheme)
    {
        if (Application.Current is null) return;
        IResourceDictionary res = Application.Current.Resources;

        void SetColor(string key, Color c) => res[key] = c;
        void SetBrush(string key, Color c, double opacity = 1.0) => res[key] = new SolidColorBrush(c, opacity);

        SetBrush("AccentBrush", scheme.Accent);
        SetBrush("AccentBrightBrush", scheme.AccentBright);
        SetBrush("AccentDeepBrush", scheme.AccentDeep);
        SetBrush("InkOnAccentBrush", scheme.InkOnAccent);
        SetBrush("TextOnAccentBrush", scheme.InkOnAccent);
        SetBrush("AccentButtonForeground", scheme.AccentBright);
        SetBrush("AccentButtonBackground", scheme.Accent, 0.16);

        SetBrush("BgDeepBrush", scheme.BgDeep);
        SetBrush("BgBaseBrush", scheme.BgBase);
        SetBrush("BgRaisedBrush", scheme.BgRaised);
        SetBrush("BgRaisedHoverBrush", scheme.BgRaisedHover);
        SetBrush("BgSidebarBrush", scheme.BgSidebar);
        SetBrush("BgInsetBrush", scheme.BgInset);

        SetBrush("HairlineBrush", scheme.Border);
        SetBrush("HairlineBrightBrush", scheme.BorderBright);
        SetBrush("HairlineStrongBrush", scheme.BorderStrong);

        SetBrush("TextPrimaryBrush", scheme.TextPrimary);
        SetBrush("TextSecondaryBrush", scheme.TextSecondary);
        SetBrush("TextMutedBrush", scheme.TextMuted);

        SetBrush("SuccessBrush", scheme.Success);
        SetBrush("WarnBrush", scheme.Warn);
        SetBrush("DangerBrush", scheme.Danger);
        SetBrush("InfoBrush", scheme.Info);

        // Fluent's own built-in controls (ToggleSwitch, etc.) read these directly.
        SetColor("SystemAccentColor", scheme.Accent);
        SetColor("SystemAccentColorLight1", scheme.AccentBright);
        SetColor("SystemAccentColorLight2", Lighten(scheme.Accent, 0.20));
        SetColor("SystemAccentColorLight3", Lighten(scheme.Accent, 0.28));
        SetColor("SystemAccentColorDark1", scheme.AccentDeep);
        SetColor("SystemAccentColorDark2", Lighten(scheme.Accent, -0.24));
        SetColor("SystemAccentColorDark3", Lighten(scheme.Accent, -0.32));
    }

    private static double RelativeLuminance(Color c)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static Color Lighten(Color c, double amount)
    {
        HslColor hsl = c.ToHsl();
        return new HslColor(hsl.A, hsl.H, hsl.S, Math.Clamp(hsl.L + amount, 0, 1)).ToRgb();
    }
}
