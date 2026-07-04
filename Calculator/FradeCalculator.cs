using System.Collections.Generic;
using System.Linq;

namespace YuCanvas.Calculator;

public static class GradeCalculator
{
    private static readonly List<(int MinPoints, string Grade)> Thresholds = new()
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
        foreach ((int minPoints, string grade) in Thresholds)
        {
            if (pointsAboveBasics >= minPoints)
                return grade;
        }
        return "3-";
    }
}
