
namespace Tms.Api.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Email { get; set; }
    public int RoleId { get; set; }
    public DateTime? CreatedOn { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Enrollment> ManagedEnrollments { get; set; } = new List<Enrollment>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
