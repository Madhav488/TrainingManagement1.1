using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tms.Api.Models;

public class Batch
{
    public int BatchId { get; set; }

    [Required]
    [ForeignKey("Calendar")]
    public int CalendarId { get; set; }

    [Required]
    [StringLength(100)]
    public string BatchName { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public CourseCalendar? Calendar { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
