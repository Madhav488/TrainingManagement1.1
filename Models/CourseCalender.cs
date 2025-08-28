
using System.Text.Json.Serialization;

namespace Tms.Api.Models;

public class CourseCalendar
{
    public int CalendarId { get; set; }
    public int CourseId { get; set; }
    public DateTime StartDate { get; set; }   // mapped to SQL 'date'
    public DateTime EndDate { get; set; }     // mapped to SQL 'date'

    // Navigation
    [JsonIgnore]
    public Course Course { get; set; } = null!;
    [JsonIgnore]
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
