using System.Text.Json.Serialization;

namespace YuCanvas.Json;

public class CanvasSubmission
{
    [JsonPropertyName("workflow_state")]
    public string WorkflowState { get; set; } = "";

    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("grade")]
    public string? Grade { get; set; }
}
