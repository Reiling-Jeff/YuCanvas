using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YuCanvas.Json;

public class CanvasCourse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("course_code")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("teachers")]
    public List<CanvasTeacher> Teachers { get; set; } = new();
    
    public List<CanvasAssignment>? Assignments { get; set; }
}

public class CanvasTeacher
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";
}
