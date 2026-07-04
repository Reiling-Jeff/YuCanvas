using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using YuCanvas.Json;

namespace YuCanvas.Service;

public class CanvasService
{
    private readonly HttpClient _http;

    public CanvasService(string baseUrl, string token)
    {
        _http = new HttpClient { BaseAddress = new System.Uri(baseUrl) };
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<CanvasCourse>> GetCoursesAsync()
    {
        List<CanvasCourse>? result = await _http.GetFromJsonAsync<List<CanvasCourse>>(
            "/api/v1/courses?enrollment_state=active&include[]=teachers");
        return result ?? new List<CanvasCourse>();
    }
    
    public async Task<List<CanvasAssignment>> GetAssignmentsAsync(long courseId)
    {
        List<CanvasAssignment>? result = await _http.GetFromJsonAsync<List<CanvasAssignment>>(
            $"/api/v1/courses/{courseId}/assignments?include[]=submission&per_page=100");
        return result ?? new List<CanvasAssignment>();
    }
}
