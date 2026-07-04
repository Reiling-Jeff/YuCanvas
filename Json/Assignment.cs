using System;
using System.Text.Json.Serialization;

namespace YuCanvas.Json;

public class Assignment
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("due_at")]
    public DateTime? DueAt { get; set; }
}
