using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuCanvas.Json;
using YuCanvas.Media;

namespace YuCanvas.Service;

public class CanvasService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex NextLinkPattern = new("<([^>]+)>;\\s*rel=\"next\"", RegexOptions.Compiled);

    private readonly HttpClient _http;

    public CanvasService(string baseUrl, string token)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Canvas:BaseUrl ist nicht konfiguriert.");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? uri))
            throw new InvalidOperationException($"Canvas:BaseUrl ist keine gültige URL: '{baseUrl}'.");

        _http = new HttpClient
        {
            BaseAddress = uri,
            Timeout = TimeSpan.FromSeconds(60)
        };
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public Task<List<Course>> GetCoursesAsync()
    {
        return GetPagedListAsync<Course>("/api/v1/courses?enrollment_state=active&include[]=teachers&per_page=100");
    }

    public async Task<StudentData> GetStudentDataAsync()
    {
        return await _http.GetFromJsonAsync<StudentData>("/api/v1/users/self") ?? new StudentData();
    }

    public Task<List<CanvasAssignment>> GetAssignmentsAsync(long courseId)
    {
        return GetPagedListAsync<CanvasAssignment>($"/api/v1/courses/{courseId}/assignments?include[]=submission&per_page=100");
    }

    public Task<List<CanvasAssignmentGroup>> GetAssignmentGroupsAsync(long courseId)
    {
        return GetPagedListAsync<CanvasAssignmentGroup>($"/api/v1/courses/{courseId}/assignment_groups?per_page=100");
    }

    public void Dispose() => _http.Dispose();

    private async Task<List<T>> GetPagedListAsync<T>(string url)
    {
        List<T> result = new();
        string? next = url;
        int pageGuard = 0;

        while (next != null && pageGuard++ < 50)
        {
            using HttpResponseMessage response = await _http.GetAsync(next);
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            List<T>? page = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
            if (page != null)
                result.AddRange(page);

            next = GetNextPageUrl(response);
        }

        return result;
    }

    private static string? GetNextPageUrl(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Link", out IEnumerable<string>? values))
            return null;

        foreach (string value in values)
        {
            Match match = NextLinkPattern.Match(value);
            if (match.Success)
                return match.Groups[1].Value;
        }

        return null;
    }
}
