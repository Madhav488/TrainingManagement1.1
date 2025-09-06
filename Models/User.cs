
namespace Tms.Api.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Email { get; set; }
    public int RoleId { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? FirstName { get; set; }   // 👈 add
    public string? LastName { get; set; }
    public int? ManagerId { get; set; }
    // Navigation
    public Role Role { get; set; } = null!;
    public User? Manager { get; set; }    // 🔹 new nav
    public ICollection<User> Employees { get; set; } = new List<User>(); // 🔹 inverse
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Enrollment> ManagedEnrollments { get; set; } = new List<Enrollment>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
