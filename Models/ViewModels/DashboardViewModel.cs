using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
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
    private int _overallProgress;
    
    private static readonly HashSet<long> _basicCollectiveDeadlineIds = new()
    {
        176092
    };
    
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

    public ObservableCollection<Deadline> Deadlines { get; } = new();
    public ObservableCollection<Announcement> Announcements { get; } = new()
    {
        //new Announcement { Title = "Klausurtermine online",         Meta = "Verteilte Systeme · vor 2 Std", IsUnread = true },
        //new Announcement { Title = "Neue Materialien in Modul 5",   Meta = "Datenbanksysteme · gestern",    IsUnread = false },
    };
    
    // --- Load Data ---
    
    public void ApplyCachedCourses(List<Course> courses)
    {
        if (courses.Count == 0)
            return;

        LoadCardValues(courses);

        Courses.Clear();
        _overallProgress = 0;
        
        ApplyCourses(courses.ToArray());

        ProgressInPercentage = courses.Count > 0 ? _overallProgress / courses.Count : 0;
        LastSyncText = "Synchronisiert...";
    }

    public void ApplyCanvasCourses(List<Course> canvasCourses)
    {
        Courses.Clear();
        _overallProgress = 0; 
        
        ApplyCourses(canvasCourses.ToArray());

        LoadCardValues(canvasCourses);

        ProgressInPercentage = canvasCourses.Count > 0 ? _overallProgress / canvasCourses.Count : 0;
        PassedSync = true;
        LastSyncText = $"Letzte Synchronisierung · {DateTime.Now:dd.MM.yyyy HH:mm}";
    }

    private void ApplyCourses(Course[] courses)
    {
        foreach (Course c in courses)
        {
            int progress = CalculateBasicProgress(c.Assignments!);
            _overallProgress += progress;

            c.Progress = progress;

            Courses.Add(c);
        }
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
                NextAssignmentText = $"Abgabe „{next.Name}“ bis {deadline:dd.MM.yy} um {deadline:HH:mm} Uhr.";
                _heroAssignment = next;
            }
            else
            {
                _heroAssignment = null;
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
                Relative = FormatDeadline(canvasAssignment.DueAt.Value),
                Source   = canvasAssignment
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
