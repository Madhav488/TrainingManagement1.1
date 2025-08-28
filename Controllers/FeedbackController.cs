using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tms.Api.Data;
using Tms.Api.Models;

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
    public async Task<ActionResult<Feedback>> Submit(int batchId, [FromBody] Feedback feedback)
    {
        if (feedback.Rating is < 1 or > 5) return BadRequest("Rating must be 1-5.");
        feedback.UserId = CurrentUserId;
        feedback.BatchId = batchId;
        feedback.SubmittedOn = DateTime.UtcNow;
        _db.Feedback.Add(feedback);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForBatch), new { batchId }, feedback);
    }

    [HttpGet("batch/{batchId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetForBatch(int batchId)
        => await _db.Feedback.Where(f => f.BatchId == batchId)
                             .Include(f => f.User)
                             .AsNoTracking().ToListAsync();
}
