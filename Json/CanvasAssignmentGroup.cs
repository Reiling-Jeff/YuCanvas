using System.Text.Json.Serialization;

namespace YuCanvas.Json;

public class CanvasAssignmentGroup
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
