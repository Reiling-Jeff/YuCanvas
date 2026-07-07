using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using YuCanvas.Json;

namespace YuCanvas.Media;

public class Course
{
    public long Id { get; set; }

    [JsonPropertyName("course_code")]
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public List<CanvasTeacher> Teachers { get; set; } = new();

    [JsonPropertyName("access_restricted_by_date")]
    public bool AccessRestrictedByDate { get; set; }

    public int Progress { get; set; }

    public List<CanvasAssignment>? Assignments { get; set; }

    public List<CanvasAssignmentGroup>? AssignmentGroups { get; set; }

    [JsonIgnore]
    public string TeacherName => Teachers?.FirstOrDefault()?.DisplayName ?? "—";

    [JsonIgnore]
    public string ShortCode
    {
        get
        {
            string source = string.IsNullOrWhiteSpace(Code) ? Name : Code;
            if (string.IsNullOrWhiteSpace(source))
                return "??";

            source = source.Trim();
            return source.Length >= 2 ? source[..2].ToUpperInvariant() : source.ToUpperInvariant();
        }
    }
}
