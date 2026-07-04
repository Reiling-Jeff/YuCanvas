using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using YuCanvas.Json;

namespace YuCanvas.Models;

public class AssignmentsViewModel
{
    public ObservableCollection<AssignmentItem> Assignments { get; } = new();

    public void Load(List<CanvasCourse> courses)
    {
        Assignments.Clear();

        foreach (CanvasCourse course in courses)
        {
            if (course.Assignments == null)
                continue;

            foreach (CanvasAssignment a in course.Assignments)
            {
                Assignments.Add(new AssignmentItem
                {
                    Name        = a.Name,
                    CourseName  = course.Name,
                    StatusText  = StatusFor(a),
                    StatusColor = ColorFor(a)
                });
            }
        }
    }

    private static string StatusFor(CanvasAssignment a) =>
        a.Submission?.WorkflowState switch
        {
            "graded"      => "Bewertet",
            "submitted"   => "Abgegeben",
            "unsubmitted" => "Offen",
            _             => "—"
        };

    private static IBrush ColorFor(CanvasAssignment a) =>
        a.Submission?.WorkflowState switch
        {
            "graded"      => new SolidColorBrush(Color.Parse("#34D399")),
            "submitted"   => new SolidColorBrush(Color.Parse("#38BDF8")),
            "unsubmitted" => new SolidColorBrush(Color.Parse("#FBBF24")),
            _             => new SolidColorBrush(Color.Parse("#6B7899"))
        };
}
