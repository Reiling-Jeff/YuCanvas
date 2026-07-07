using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Service;

public class SyncResult
{
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public List<Course> Courses { get; set; } = new();
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

                if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
                {
                    appSettings.CanvasBaseUrl = baseUrl;
                    appSettings.CanvasToken = token;
                    await SettingsService.SaveAsync(appSettings);
                }
            }

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(token))
            {
                return new SyncResult
                {
                    Success = false,
                    FailureReason = "Canvas-URL oder Token fehlen. Bitte in den Einstellungen hinterlegen."
                };
            }

            using CanvasService service = new CanvasService(baseUrl, token);

            List<Course> canvasCourses = await service.GetCoursesAsync();
            canvasCourses.RemoveAll(c => c.AccessRestrictedByDate);

            StudentData studentData = await service.GetStudentDataAsync();

            await Task.WhenAll(canvasCourses.Select(async c =>
            {
                try
                {
                    c.Assignments = await service.GetAssignmentsAsync(c.Id);
                    c.AssignmentGroups = await service.GetAssignmentGroupsAsync(c.Id);

                    foreach (CanvasAssignment assignment in c.Assignments)
                        assignment.Course = c.Name;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kurs {c.Id} fehlgeschlagen: {ex.Message}");
                    c.Assignments = new List<CanvasAssignment>();
                    c.AssignmentGroups = new List<CanvasAssignmentGroup>();
                }
            }));

            await CacheService.SaveStudentDataAsync(studentData);
            await CacheService.SaveCoursesAsync(canvasCourses);

            return new SyncResult
            {
                Success = true,
                Courses = canvasCourses,
                StudentData = studentData
            };
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            return new SyncResult
            {
                Success = false,
                FailureReason = "Canvas nicht erreichbar. Verbindung und Token prüfen."
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SyncResult { Success = false };
        }
    }
}
