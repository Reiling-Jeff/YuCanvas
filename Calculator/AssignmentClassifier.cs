using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Calculator;

public enum AssignmentCategory
{
    Basic,
    Intermediate,
    Advanced,
    Other
}

public static class AssignmentClassifier
{
    private const int IntermediatePoints = 2;
    private const int AdvancedPoints = 4;

    private static readonly string[] BasicKeywords = { "basic", "basis", "grundlagen" };
    private static readonly string[] IntermediateKeywords = { "intermediate", "aufbau" };
    private static readonly string[] AdvancedKeywords = { "advanced", "fortgeschritten", "expert" };

    private static readonly Regex BasicPrefix = new(@"^B\d+\.\d+", RegexOptions.Compiled);
    private static readonly Regex IntermediatePrefix = new(@"^I\d+\.\d+", RegexOptions.Compiled);
    private static readonly Regex AdvancedPrefix = new(@"^A\d+\.\d+", RegexOptions.Compiled);

    public static Dictionary<long, string> BuildGroupLookup(IEnumerable<Course> courses)
    {
        Dictionary<long, string> lookup = new();

        foreach (Course course in courses)
        {
            if (course.AssignmentGroups == null)
                continue;

            foreach (CanvasAssignmentGroup group in course.AssignmentGroups)
                lookup[group.Id] = group.Name;
        }

        return lookup;
    }

    public static AssignmentCategory Categorize(CanvasAssignment assignment, IReadOnlyDictionary<long, string> groupNames)
    {
        if (assignment.AssignmentGroupId is long groupId
            && groupNames.TryGetValue(groupId, out string? groupName))
        {
            AssignmentCategory fromGroup = CategorizeGroupName(groupName);
            if (fromGroup != AssignmentCategory.Other)
                return fromGroup;
        }

        return CategorizeAssignmentName(assignment.Name);
    }

    public static bool IsGraded(CanvasAssignment assignment) =>
        assignment.Submission?.WorkflowState == "graded";

    public static List<CanvasAssignment> GetProgressRelevant(List<CanvasAssignment> assignments, IReadOnlyDictionary<long, string> groupNames)
    {
        List<CanvasAssignment> basics = assignments
            .Where(a => Categorize(a, groupNames) == AssignmentCategory.Basic)
            .ToList();

        return basics.Count > 0 ? basics : assignments;
    }

    public static bool HasGradeRelevant(IEnumerable<CanvasAssignment> assignments, IReadOnlyDictionary<long, string> groupNames) =>
        assignments.Any(a =>
        {
            AssignmentCategory category = Categorize(a, groupNames);
            return category is AssignmentCategory.Intermediate or AssignmentCategory.Advanced;
        });

    public static int CalculatePoints(IEnumerable<CanvasAssignment> assignments, IReadOnlyDictionary<long, string> groupNames)
    {
        int points = 0;

        foreach (CanvasAssignment assignment in assignments)
        {
            if (!IsGraded(assignment))
                continue;

            switch (Categorize(assignment, groupNames))
            {
                case AssignmentCategory.Intermediate:
                    points += IntermediatePoints;
                    break;
                case AssignmentCategory.Advanced:
                    points += AdvancedPoints;
                    break;
            }
        }

        return points;
    }

    private static AssignmentCategory CategorizeGroupName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AssignmentCategory.Other;

        string lower = name.ToLowerInvariant();

        if (BasicKeywords.Any(lower.Contains))
            return AssignmentCategory.Basic;
        if (IntermediateKeywords.Any(lower.Contains))
            return AssignmentCategory.Intermediate;
        if (AdvancedKeywords.Any(lower.Contains))
            return AssignmentCategory.Advanced;

        return CategorizeByPrefix(name);
    }

    private static AssignmentCategory CategorizeAssignmentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AssignmentCategory.Other;

        AssignmentCategory fromPrefix = CategorizeByPrefix(name);
        if (fromPrefix != AssignmentCategory.Other)
            return fromPrefix;

        string lower = name.ToLowerInvariant();

        if (BasicKeywords.Any(lower.Contains))
            return AssignmentCategory.Basic;
        if (IntermediateKeywords.Any(lower.Contains))
            return AssignmentCategory.Intermediate;
        if (AdvancedKeywords.Any(lower.Contains))
            return AssignmentCategory.Advanced;

        return AssignmentCategory.Other;
    }

    private static AssignmentCategory CategorizeByPrefix(string name)
    {
        if (BasicPrefix.IsMatch(name))
            return AssignmentCategory.Basic;
        if (IntermediatePrefix.IsMatch(name))
            return AssignmentCategory.Intermediate;
        if (AdvancedPrefix.IsMatch(name))
            return AssignmentCategory.Advanced;

        return AssignmentCategory.Other;
    }
}
