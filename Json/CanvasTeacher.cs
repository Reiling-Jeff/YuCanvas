using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YuCanvas.Json;

public class CanvasTeacher
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";
}
