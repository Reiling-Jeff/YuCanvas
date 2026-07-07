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
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private static readonly string _coursesFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YuCanvas",
        "courses.json");

    private static readonly string _studentDataFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YuCanvas",
        "studentData.json");

    public static async Task SaveCoursesAsync(List<Course> courses)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_coursesFile)!);
            string json = JsonSerializer.Serialize(courses, WriteOptions);
            await File.WriteAllTextAsync(_coursesFile, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static async Task SaveStudentDataAsync(StudentData studentData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_studentDataFile)!);
            string json = JsonSerializer.Serialize(studentData, WriteOptions);
            await File.WriteAllTextAsync(_studentDataFile, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static async Task<List<Course>> LoadCoursesAsync()
    {
        if (!File.Exists(_coursesFile))
            return new List<Course>();

        try
        {
            string json = await File.ReadAllTextAsync(_coursesFile);
            return JsonSerializer.Deserialize<List<Course>>(json) ?? new List<Course>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<Course>();
        }
    }

    public static async Task<StudentData> LoadStudentDataAsync()
    {
        if (!File.Exists(_studentDataFile))
            return new StudentData();

        try
        {
            string json = await File.ReadAllTextAsync(_studentDataFile);
            return JsonSerializer.Deserialize<StudentData>(json) ?? new StudentData();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new StudentData();
        }
    }
}
