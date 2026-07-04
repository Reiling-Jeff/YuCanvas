using System;
using System.Text.Json.Serialization;
using YuCanvas.Json;

namespace YuCanvas.Json;

public class CanvasAssignment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("course")] 
    public string Course { get; set; } = "";

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("due_at")]
    public DateTime? DueAt { get; set; }

    [JsonPropertyName("submission")]
    public CanvasSubmission? Submission { get; set; }
}
