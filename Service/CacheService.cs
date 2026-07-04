using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Service;

public static class CacheService
{
    private static readonly string _coursesFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YuCanvas",
        "courses.json");
    
    private static readonly string _assignmentsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YuCanvas",
        "assignments.json");

    public static async Task SaveAssignmentsAsync(List<CanvasAssignment> assignments)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_assignmentsFile)!);
        string json = JsonSerializer.Serialize(assignments);
        await File.WriteAllTextAsync(_assignmentsFile, json);
    }
    
    public static async Task<List<CanvasAssignment>> LoadAssignmentsAsync()
    {
        if (!File.Exists(_assignmentsFile))
            return new List<CanvasAssignment>();

        string json = await File.ReadAllTextAsync(_assignmentsFile);
        return JsonSerializer.Deserialize<List<CanvasAssignment>>(json) ?? new List<CanvasAssignment>();
    }
    
    public static async Task SaveCoursesAsync(List<Course> courses)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_coursesFile)!);
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(courses, options);
        await File.WriteAllTextAsync(_coursesFile, json);
    }

    public static async Task<List<Course>> LoadCoursesAsync()
    {
        if (!File.Exists(_coursesFile))
            return new List<Course>();

        string json = await File.ReadAllTextAsync(_coursesFile);
        return JsonSerializer.Deserialize<List<Course>>(json) ?? new List<Course>();
    }
}
