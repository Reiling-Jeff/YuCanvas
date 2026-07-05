using System.Collections.Generic;
using YuCanvas.Models.ViewModels;

namespace YuCanvas.Calculator;

public static class GradeCalculator
{
    public static readonly IReadOnlyList<(int MinPoints, string Grade)> DefaultThresholds = new List<(int, string)>
    {
        (64, "1++"),
        (54, "1+"),
        (46, "1"),
        (38, "1-"),
        (32, "2.1+"),
        (26, "2.1"),
        (20, "2.1-"),
        (16, "2.2+"),
        (12, "2.2"),
        (8,  "2.2-"),
        (5,  "3+"),
        (2,  "3"),
        (0,  "3-"),
    };

    public static string GetGrade(int pointsAboveBasics)
    {
        IReadOnlyList<(int MinPoints, string Grade)> thresholds = GetActiveThresholds();

        foreach ((int minPoints, string grade) in thresholds)
        {
            if (pointsAboveBasics >= minPoints)
                return grade;
        }

        return thresholds.Count > 0 ? thresholds[^1].Grade : "n/a";
    }

    private static IReadOnlyList<(int MinPoints, string Grade)> GetActiveThresholds()
    {
        List<GradeThresholdEntry>? custom = SettingsViewModel.Settings.GradeThresholdEntries;
        if (custom is not { Count: > 0 })
            return DefaultThresholds;

        List<(int MinPoints, string Grade)> parsed = new();
        foreach (GradeThresholdEntry entry in custom)
        {
            if (string.IsNullOrWhiteSpace(entry.Label))
                continue;
            if (!int.TryParse(entry.MinPoints, out int minPoints))
                continue;
            parsed.Add((minPoints, entry.Label.Trim()));
        }

        if (parsed.Count == 0)
            return DefaultThresholds;

        parsed.Sort((a, b) => b.MinPoints.CompareTo(a.MinPoints));
        return parsed;
    }
}
