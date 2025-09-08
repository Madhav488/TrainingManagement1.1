using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tms.Api.Data;
using Tms.Api.Models;
using Tms.Api.Dtos;
namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly TmsDbContext _db;
    public FeedbackController(TmsDbContext db) => _db = db;

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpPost("{batchId:int}")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<Feedback>> Submit(int batchId, [FromBody] FeedbackCreateDto dto)
    {
        if (dto.Rating is < 1 or > 5)
            return BadRequest("Rating must be 1-5.");

        var batch = await _db.Batch.Include(b => b.Calendar)
                                   .FirstOrDefaultAsync(b => b.BatchId == batchId);
        if (batch == null) return NotFound("Batch not found.");
        if (batch.Calendar.EndDate > DateTime.UtcNow.Date)
            return BadRequest("Feedback can only be submitted after the batch has finished.");

        bool alreadySubmitted = await _db.Feedback
            .AnyAsync(f => f.BatchId == batchId && f.UserId == CurrentUserId);
        if (alreadySubmitted)
            return BadRequest("You have already submitted feedback for this batch.");

        var feedback = new Feedback
        {
            FeedbackText = dto.FeedbackText,
            Rating = dto.Rating,
            UserId = CurrentUserId,
            BatchId = batchId,
            SubmittedOn = DateTime.UtcNow
        };

        _db.Feedback.Add(feedback);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForBatch), new { batchId }, feedback);
    }


    [HttpGet("batch/{batchId:int}")]
    [Authorize(Roles = "Administrator")]   
    public async Task<ActionResult<IEnumerable<FeedbackReadDto>>> GetForBatch(int batchId)
    {
        var feedbacks = await _db.Feedback
            .Where(f => f.BatchId == batchId)
            .Include(f => f.User) 
            .AsNoTracking()
            .Select(f => new FeedbackReadDto
            {
                FeedbackId = f.FeedbackId,
                FeedbackText = f.FeedbackText,
                Rating = f.Rating,
                SubmittedOn = f.SubmittedOn,
                UserId = f.UserId,
                Username = f.User.Username 
            })
            .ToListAsync();

        return feedbacks;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<FeedbackReadDto>>> GetAll()
    {
        var feedbacks = await _db.Feedback
            .Include(f => f.User)
            .Include(f => f.Batch).ThenInclude(b => b.Calendar).ThenInclude(c => c.Course)
            .Select(f => new FeedbackReadDto
            {
                FeedbackId = f.FeedbackId,
                FeedbackText = f.FeedbackText,
                Rating = f.Rating,
                SubmittedOn = f.SubmittedOn,
                UserId = f.UserId,
                Username = f.User.Username,
                CourseName = f.Batch.Calendar.Course.CourseName,   // ✅ NEW
                BatchName = f.Batch.BatchName
            })
            .ToListAsync();

        return feedbacks;
    }

}
