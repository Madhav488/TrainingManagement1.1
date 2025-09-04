namespace Tms.Api.Models;

public class Feedback
{
    public int FeedbackId { get; set; }
    public int UserId { get; set; }
    public int BatchId { get; set; }
    public string? FeedbackText { get; set; }
    public int? Rating { get; set; }            // CHECK (Rating BETWEEN 1 AND 5); nullable per SQL
    public DateTime? SubmittedOn { get; set; }

    // Navigation
    public User User { get; set; }
    public Batch Batch { get; set; }
}
