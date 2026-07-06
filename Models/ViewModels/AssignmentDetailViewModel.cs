using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YuCanvas.Json;

namespace YuCanvas.Models.ViewModels;

public partial class AssignmentDetailViewModel : ObservableObject
{
    private readonly CanvasAssignment _assignment;

    public AssignmentDetailViewModel(CanvasAssignment assignment)
    {
        _assignment = assignment;
    }

    public string Name => _assignment.Name;

    public long Id => _assignment.Id;

    public string DeadlineText => _assignment.DueAt.HasValue
        ? $"Fällig am {_assignment.DueAt.Value:dd.MM.yyyy} um {_assignment.DueAt.Value:HH:mm} Uhr"
        : "Keine Deadline";

    public string PointsText => _assignment.PointsPossible.HasValue
        ? $"{_assignment.PointsPossible.Value:0.#} Punkte"
        : "—";

    public string StatusText => _assignment.Submission?.WorkflowState switch
    {
        "graded"      => "Bewertet",
        "submitted"   => "Abgegeben",
        "unsubmitted" => "Offen",
        _             => "—"
    };

    public string GradeText
    {
        get
        {
            CanvasSubmission? sub = _assignment.Submission;
            if (sub?.Grade != null)
                return $"Note: {sub.Grade}";
            if (sub?.Score != null)
                return $"Punkte erreicht: {sub.Score:0.#}";
            return "Noch nicht bewertet";
        }
    }

    public string DescriptionHtml => _assignment.Description ?? "<p>Keine Beschreibung vorhanden.</p>";

    public string? HtmlUrl => _assignment.HtmlUrl;

    public event Action? BackRequested;

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke();

    [RelayCommand]
    private void OpenInBrowser()
    {
        if (string.IsNullOrEmpty(_assignment.HtmlUrl))
            return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _assignment.HtmlUrl,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
