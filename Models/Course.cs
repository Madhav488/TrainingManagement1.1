

using System.Text.Json.Serialization;

namespace Tms.Api.Models;

public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public string? Description { get; set; }
    public int? DurationDays { get; set; }
    public DateTime? CreatedOn { get; set; }

    // Navigation
    [JsonIgnore]
    public ICollection<CourseCalendar> Calendars { get; set; } = new List<CourseCalendar>();
}
