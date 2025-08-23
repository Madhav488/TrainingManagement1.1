namespace Tms.Api.Models;

public class Batch
{
    public int BatchId { get; set; }
    public int CalendarId { get; set; }
    public string BatchName { get; set; } = null!;
    public DateTime? CreatedOn { get; set; }

    // Navigation
    public CourseCalendar Calendar { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
