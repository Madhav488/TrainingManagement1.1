namespace Tms.Api.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public int BatchId { get; set; }
    public string? Status { get; set; }          // CHECK (Status IN (...)); nullable per SQL
    public int? ManagerId { get; set; }
    public DateTime? RequestedOn { get; set; }
    //try with enum
    // Navigation
    public User User { get; set; } = null!;
    public User? Manager { get; set; }
    public Batch Batch { get; set; } = null!;
}
