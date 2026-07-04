using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using YuCanvas.Calculator;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Models;

public partial class DashboardViewModel : ObservableObject
{
    private static readonly HashSet<long> _basicCollectiveDeadlineIds = new()
    {
        176092
    };
    
    // --- Top Card ---
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SyncPassedColor))]
    private bool _passedSync = false;
    
    public IBrush SyncPassedColor => _passedSync 
                                     ? new SolidColorBrush(Color.Parse("#34D399")) 
                                     : new SolidColorBrush(Color.Parse("#FB7185"));

    [ObservableProperty]
    private string _lastSyncText = "n/a";
    [ObservableProperty]
    private string _nextAssignmentDeadline = "n/a";
    [ObservableProperty]
    private string _nextAssignmentText = "n/a";
    
    // --- Overview Cards ---
    [ObservableProperty]
    private string _averageGrade = "n/a";
    [ObservableProperty]
    private string _diffAverageGradeLastSemester = "+0,3 zum Vorsemester";
    [ObservableProperty]
    private int _remainingAssignments = 0;
    [ObservableProperty]
    private string _deadlinesThisWeekText = "2 fällig diese Woche";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressInPercentageText))]
    private static int _progressInPercentage = 0;
    
    public string ProgressInPercentageText => $"{_progressInPercentage}%";
    
    // --- Your Courses ---
    public ObservableCollection<Course> Courses { get; } = new();
    
    public ObservableCollection<Deadline> Deadlines { get; } = new()
    {
        new Deadline { Title = "Übung 6 abgeben", Course = "Verteilte Systeme",
            DueLabel = "Sa 23:59", Relative = "in 2 Tagen",
            AccentColor = new SolidColorBrush(Color.Parse("#FB7185")) },
        new Deadline { Title = "Quiz Kapitel 4",  Course = "Datenbanksysteme",
            DueLabel = "Mo 12:00", Relative = "in 4 Tagen",
            AccentColor = new SolidColorBrush(Color.Parse("#FBBF24")) },
        new Deadline { Title = "Projektabgabe",   Course = "Algorithmen",
            DueLabel = "12. Jul",  Relative = "in 9 Tagen",
            AccentColor = new SolidColorBrush(Color.Parse("#38BDF8")) },
    };
    
    public ObservableCollection<Announcement> Announcements { get; } = new()
    {
        new Announcement { Title = "Klausurtermine online",         Meta = "Verteilte Systeme · vor 2 Std", IsUnread = true },
        new Announcement { Title = "Neue Materialien in Modul 5",   Meta = "Datenbanksysteme · gestern",    IsUnread = false },
    };
    
    // --- Load Data ---
    
    public void ApplyCachedCourses(List<Course> courses)
    {
        if (courses.Count == 0)
            return;

        LoadCardValues(courses);

        Courses.Clear();
        int overallProgress = 0;
        foreach (Course c in courses)
        {
            overallProgress += c.Progress;
            Courses.Add(c);
        }

        ProgressInPercentage = courses.Count > 0 ? overallProgress / courses.Count : 0;
        LastSyncText = "Synchronisiert...";
    }

    public void ApplyCanvasCourses(List<CanvasCourse> canvasCourses)
    {
        Courses.Clear();
        int overallProgress = 0;

        foreach (CanvasCourse c in canvasCourses)
        {
            int progress = CalculateBasicProgress(c.Assignments!);
            overallProgress += progress;

            Courses.Add(new Course
            {
                Code        = ShortCode(c.CourseCode),
                Name        = c.Name,
                Lecturer    = c.Teachers.FirstOrDefault()?.DisplayName ?? "Unbekannt",
                Progress    = progress,
                Assignments = c.Assignments
            });
        }

        LoadCardValues(canvasCourses);

        ProgressInPercentage = canvasCourses.Count > 0 ? overallProgress / canvasCourses.Count : 0;
        PassedSync = true;
        LastSyncText = $"Letzte Synchronisierung ·  {DateTime.Now:dd.MM.yyyy HH:mm}";
    }

    public void MarkSyncFailed()
    {
        PassedSync = false;
        LastSyncText = "Sync failed. Contact mail@yuuto.me";
    }

    private static int CalculatePointsAboveBasics(List<CanvasAssignment> assignments)
    {
        int points = 0;
        foreach (CanvasAssignment a in assignments)
        {
            if (a.Submission?.WorkflowState != "graded")
                continue;

            if (Regex.IsMatch(a.Name, @"^I\d+\.\d+"))
                points += 2;
            else if (Regex.IsMatch(a.Name, @"^A\d+\.\d+"))
                points += 4;
        }
        return points;
    }
    
    private int CalculateBasicProgress(List<CanvasAssignment> assignments)
    {
        List<CanvasAssignment> basics = assignments.Where(a => IsBasic(a.Name)).ToList();
        if (basics.Count == 0) return 0;

        int done = basics.Count(a => a.Submission?.WorkflowState == "graded");
        return (int)Math.Round(done * 100.0 / basics.Count);
    }

    private void LoadCardValues(List<CanvasCourse> courses)
    {
        List<CanvasAssignment> assignments = new List<CanvasAssignment>();

        foreach (CanvasCourse course in courses)
        {
            if (course.Assignments != null)
                assignments.AddRange(course.Assignments);
        }
        
        LoadCardValues(assignments);
    }
    
    private void LoadCardValues(List<Course> courses)
    {
        List<CanvasAssignment> assignments = new List<CanvasAssignment>();

        foreach (Course course in courses)
        {
            if (course.Assignments != null)
                assignments.AddRange(course.Assignments);
        }
        
        LoadCardValues(assignments);
    }

    private void LoadCardValues(List<CanvasAssignment> assignments)
    {
        if (TryGetNextDeadline(assignments, out DateTime deadline, out CanvasAssignment? next))
        {
            if (next != null)
            {
                NextAssignmentDeadline = $"Deine nächste Deadline ist {FormatDeadline(deadline)}";
                NextAssignmentText = $"Abgabe „{next.Name}“ bis {deadline.ToString("dd.MM.yy")} um {deadline.ToString("HH:mm")} Uhr.";
            }
            else
            {
                Console.WriteLine($"Sammelfrist {deadline:dd.MM.yyyy}, aber keine offene Basic mehr");
            }
        }
        
        List<CanvasAssignment> assignmentsWithDeadline = GetAllDeadlines(assignments);

        Deadlines.Clear();
        
        foreach (CanvasAssignment canvasAssignment in assignmentsWithDeadline )
        {
            Deadlines.Add(new Deadline { 
                Title    = canvasAssignment.Name, 
                Course   = canvasAssignment.Course,
                DueLabel = canvasAssignment.DueAt!.Value.ToString("dd.MM.yy HH:mm"), 
                Relative = FormatDeadline(canvasAssignment.DueAt.Value)
            });
        }
        
        List<CanvasAssignment> basics = assignments.Where(a => IsBasic(a.Name)).ToList();
        if (basics.Count == 0) return;
        
        RemainingAssignments = basics.Count(a => a.Submission?.WorkflowState == "unsubmitted");
        int abovePoints = CalculatePointsAboveBasics(assignments);
        AverageGrade = GradeCalculator.GetGrade(abovePoints);
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
    
    private static bool IsBasic(string name) => Regex.IsMatch(name, @"^B\d+\.\d+");

    public static bool TryGetNextDeadline(List<CanvasAssignment> assignments, out DateTime deadline, out CanvasAssignment? assignment)
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

        if (_basicCollectiveDeadlineIds.Contains(next.Id))
        {
            CanvasAssignment? nextBasic = assignments
                .Where(a => Regex.IsMatch(a.Name, @"^B\d+\.\d+"))
                .Where(a => a.Submission?.WorkflowState != "graded")
                .OrderBy(a => a.Position)
                .FirstOrDefault();

            assignment = nextBasic;
        }
        else
        {
            assignment = next;
        }

        return true;
    }
    
    public static List<CanvasAssignment> GetAllDeadlines(List<CanvasAssignment> assignments)
    {
        List<CanvasAssignment> entries = new List<CanvasAssignment>();

        foreach (CanvasAssignment a in assignments)
        {
            if (a.DueAt == null)
                continue;

            if (_basicCollectiveDeadlineIds.Contains(a.Id))
            {
                IOrderedEnumerable<CanvasAssignment> openBasics = assignments
                    .Where(b => Regex.IsMatch(b.Name, @"^B\d+\.\d+"))
                    .Where(b => b.Submission?.WorkflowState != "graded" && b.Id != a.Id)
                    .OrderBy(b => b.Position);

                foreach (CanvasAssignment basic in openBasics)
                    basic.DueAt = a.DueAt;
                
                entries.AddRange(openBasics);
            }
            else
            {
                entries.Add(a);
            }
        }

        return entries.OrderBy(e => e.DueAt).ToList();
    }

    private static string ShortCode(string courseCode)
    {
        if (string.IsNullOrWhiteSpace(courseCode)) return "??";
        return courseCode.Length >= 2 ? courseCode[..2].ToUpper() : courseCode.ToUpper();
    }
}
