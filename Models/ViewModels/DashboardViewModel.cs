using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Calculator;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Models.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private CanvasAssignment? _heroAssignment;
    private static AppSettings AppSettings => SettingsViewModel.Settings;

    public event Action<CanvasAssignment>? AssignmentSelected;

    [RelayCommand]
    private void OpenDeadline(Deadline deadline)
    {
        if (deadline.Source != null)
            AssignmentSelected?.Invoke(deadline.Source);
    }

    [RelayCommand]
    private void OpenHeroAssignment()
    {
        if (_heroAssignment != null)
            AssignmentSelected?.Invoke(_heroAssignment);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SyncPassedColor))]
    private bool _passedSync;

    public IBrush SyncPassedColor => _passedSync
        ? new SolidColorBrush(Color.Parse("#34D399"))
        : new SolidColorBrush(Color.Parse("#FB7185"));

    [ObservableProperty]
    private string _lastSyncText = "n/a";
    [ObservableProperty]
    private string _nextAssignmentDeadline = "n/a";
    [ObservableProperty]
    private string _nextAssignmentText = "n/a";

    [ObservableProperty]
    private string _averageGrade = "n/a";
    [ObservableProperty]
    private string _averageGradeSubText = "";
    [ObservableProperty]
    private int _remainingAssignments;
    [ObservableProperty]
    private string _deadlinesThisWeekText = "0 fällig diese Woche";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressInPercentageText))]
    private int _progressInPercentage;

    public string ProgressInPercentageText => $"{_progressInPercentage}%";

    public ObservableCollection<Course> Courses { get; } = new();

    public ObservableCollection<Deadline> Deadlines { get; } = new();
    public ObservableCollection<Announcement> Announcements { get; } = new();

    public void ApplyCachedCourses(List<Course> courses)
    {
        if (courses.Count == 0)
            return;

        Apply(courses);
        LastSyncText = "Synchronisiert...";
    }

    public void ApplyCanvasCourses(List<Course> canvasCourses)
    {
        Apply(canvasCourses);
        PassedSync = true;
        LastSyncText = $"Letzte Synchronisierung · {DateTime.Now:dd.MM.yyyy HH:mm}";
    }

    public void MarkSyncFailed(string? reason = null)
    {
        PassedSync = false;
        LastSyncText = reason ?? "Sync failed. Contact mail@yuuto.me";
    }

    private void Apply(List<Course> courses)
    {
        Dictionary<long, string> groupNames = AssignmentClassifier.BuildGroupLookup(courses);

        Courses.Clear();

        int doneOverall = 0;
        int totalOverall = 0;

        foreach (Course course in courses)
        {
            List<CanvasAssignment> assignments = course.Assignments ?? new List<CanvasAssignment>();
            List<CanvasAssignment> relevant = AssignmentClassifier.GetProgressRelevant(assignments, groupNames);

            int done = relevant.Count(AssignmentClassifier.IsGraded);
            course.Progress = relevant.Count > 0 ? (int)Math.Round(done * 100.0 / relevant.Count) : 0;

            doneOverall += done;
            totalOverall += relevant.Count;

            Courses.Add(course);
        }

        ProgressInPercentage = totalOverall > 0 ? (int)Math.Round(doneOverall * 100.0 / totalOverall) : 0;
        RemainingAssignments = totalOverall - doneOverall;

        LoadCardValues(courses, groupNames);
    }

    private void LoadCardValues(List<Course> courses, Dictionary<long, string> groupNames)
    {
        List<CanvasAssignment> assignments = new List<CanvasAssignment>();

        foreach (Course course in courses)
        {
            if (course.Assignments != null)
                assignments.AddRange(course.Assignments);
        }

        if (TryGetNextDeadline(assignments, groupNames, out DateTime deadline, out CanvasAssignment? next) && next != null)
        {
            NextAssignmentDeadline = $"Deine nächste Deadline ist {FormatDeadline(deadline)}";
            NextAssignmentText = $"Abgabe „{next.Name}“ bis {deadline:dd.MM.yy} um {deadline:HH:mm} Uhr.";
            _heroAssignment = next;
        }
        else
        {
            NextAssignmentDeadline = "Aktuell keine anstehende Deadline";
            NextAssignmentText = "Sobald eine Abgabe fällig wird, erscheint sie hier.";
            _heroAssignment = null;
        }

        List<(CanvasAssignment Assignment, DateTime DueAt)> upcoming = GetAllDeadlines(assignments, groupNames);

        Deadlines.Clear();

        foreach ((CanvasAssignment assignment, DateTime dueAt) in upcoming)
        {
            Deadlines.Add(new Deadline
            {
                Title    = assignment.Name,
                Course   = assignment.Course,
                DueLabel = dueAt.ToString("dd.MM.yy HH:mm"),
                Relative = FormatDeadline(dueAt),
                Source   = assignment
            });
        }

        DateTime now = DateTime.Now;
        int dueThisWeek = upcoming.Count(e => e.DueAt >= now && e.DueAt <= now.AddDays(7));
        DeadlinesThisWeekText = dueThisWeek == 1
            ? "1 fällig diese Woche"
            : $"{dueThisWeek} fällig diese Woche";

        if (AssignmentClassifier.HasGradeRelevant(assignments, groupNames))
        {
            int points = AssignmentClassifier.CalculatePoints(assignments, groupNames);
            AverageGrade = GradeCalculator.GetGrade(points);
            AverageGradeSubText = points == 1
                ? "1 Punkt über Basics"
                : $"{points} Punkte über Basics";
        }
        else
        {
            AverageGrade = "n/a";
            AverageGradeSubText = "Keine notenrelevanten Aufgaben";
        }
    }

    private static bool TryGetNextDeadline(List<CanvasAssignment> assignments, Dictionary<long, string> groupNames, out DateTime deadline, out CanvasAssignment? assignment)
    {
        deadline = default;
        assignment = null;

        CanvasAssignment? next = assignments
            .Where(a => a.DueAt != null)
            .OrderBy(a => a.DueAt)
            .FirstOrDefault();

        if (next == null)
            return false;

        deadline = next.DueAt!.Value;

        if (AppSettings.CollectionDeadlineEntries.Contains(next.Id))
        {
            assignment = assignments
                .Where(a => AssignmentClassifier.Categorize(a, groupNames) == AssignmentCategory.Basic)
                .Where(a => !AssignmentClassifier.IsGraded(a))
                .OrderBy(a => a.Position)
                .ThenBy(a => a.Id)
                .FirstOrDefault() ?? next;
        }
        else
        {
            assignment = next;
        }

        return true;
    }

    private static List<(CanvasAssignment Assignment, DateTime DueAt)> GetAllDeadlines(List<CanvasAssignment> assignments, Dictionary<long, string> groupNames)
    {
        List<(CanvasAssignment Assignment, DateTime DueAt)> entries = new();
        HashSet<long> seen = new();

        foreach (CanvasAssignment assignment in assignments)
        {
            if (assignment.DueAt == null)
                continue;

            if (AppSettings.CollectionDeadlineEntries.Contains(assignment.Id))
            {
                IEnumerable<CanvasAssignment> openBasics = assignments
                    .Where(b => b.Id != assignment.Id)
                    .Where(b => AssignmentClassifier.Categorize(b, groupNames) == AssignmentCategory.Basic)
                    .Where(b => !AssignmentClassifier.IsGraded(b))
                    .OrderBy(b => b.Position)
                    .ThenBy(b => b.Id);

                foreach (CanvasAssignment basic in openBasics)
                {
                    if (seen.Add(basic.Id))
                        entries.Add((basic, assignment.DueAt.Value));
                }
            }
            else if (seen.Add(assignment.Id))
            {
                entries.Add((assignment, assignment.DueAt.Value));
            }
        }

        return entries.OrderBy(e => e.DueAt).ToList();
    }

    private static string FormatDeadline(DateTime deadline)
    {
        TimeSpan remaining = deadline - DateTime.Now;

        if (remaining.TotalDays < 0)
            return "überfällig";
        if (remaining.TotalDays < 1)
            return "heute fällig";

        int days = (int)remaining.TotalDays;
        return days == 1 ? "in 1 Tag" : $"in {days} Tagen";
    }
}
