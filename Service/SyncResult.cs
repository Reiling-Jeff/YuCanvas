using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuCanvas.Json;
using YuCanvas.Media;
using YuCanvas.Models;

namespace YuCanvas.Service;

public class SyncResult
{
    public bool Success { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<CanvasCourse> CanvasCourses { get; set; } = new();
    public StudentData? StudentData { get; set; }
}

public class SyncService
{
    public async Task<SyncResult> LoadFromCacheAsync()
    {
        List<Course> cachedCourses = await CacheService.LoadCoursesAsync();
        StudentData cachedStudentData = await CacheService.LoadStudentDataAsync();

        return new SyncResult
        {
            Success = true,
            Courses = cachedCourses,
            StudentData = cachedStudentData
        };
    }

    public async Task<SyncResult> SyncFromCanvasAsync(AppSettings appSettings)
    {
        try
        {
            string baseUrl = appSettings.CanvasBaseUrl;
            string token = appSettings.CanvasToken;

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(token))
            {
                baseUrl = Program.Configuration["Canvas:BaseUrl"] ?? "";
                token = Program.Configuration["Canvas:Token"] ?? "";

                appSettings.CanvasBaseUrl = baseUrl;
                appSettings.CanvasToken = token;
                await SettingsService.SaveAsync(appSettings);
            }

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(token))
                return new SyncResult { Success = false };

            CanvasService service = new CanvasService(baseUrl, token);

            List<CanvasCourse> canvasCourses = await service.GetCoursesAsync();
            StudentData studentData = await service.GetStudentDataAsync();

            await Task.WhenAll(canvasCourses.Select(async c =>
            {
                try
                {
                    c.Assignments = await service.GetAssignmentsAsync(c.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kurs {c.Id} fehlgeschlagen: {ex.Message}");
                    c.Assignments = new List<CanvasAssignment>();
                }
            }));

            await CacheService.SaveStudentDataAsync(studentData);

            return new SyncResult
            {
                Success = true,
                CanvasCourses = canvasCourses,
                StudentData = studentData
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SyncResult { Success = false };
        }
    }
}