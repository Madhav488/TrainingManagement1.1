namespace Tms.Api.Dtos;

public class FeedbackReadDto
{
    public int FeedbackId { get; set; }
    public string? FeedbackText { get; set; }
    public int? Rating { get; set; }
    public DateTime? SubmittedOn { get; set; }

    // Optional: basic user info if you want
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
}
