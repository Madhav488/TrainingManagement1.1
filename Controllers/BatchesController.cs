using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tms.Api.Data;
using Tms.Api.Dtos;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchesController : ControllerBase
{
    private readonly TmsDbContext _db;
    public BatchesController(TmsDbContext db) => _db = db;

    // Only Employee, Manager, Admin can view
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<IEnumerable<object>>> All()
    {
        var batches = await _db.Batch
            .Where(b => b.IsActive)
            .Include(b => b.Calendar)
                .ThenInclude(c => c.Course)
            .Include(b => b.Enrollments) 
            .Include(b => b.Feedbacks)
            .AsNoTracking()
            .ToListAsync();

        // Instead of returning the whole enrollment list, project to a DTO with count
        var result = batches.Select(b => new
        {
            b.BatchId,
            b.CalendarId,
            b.BatchName,
            b.CreatedOn,
            b.IsActive,
            b.ModifiedBy,
            Calendar = b.Calendar == null ? null : new
            {
                b.Calendar.CalendarId,
                b.Calendar.CourseId,
                b.Calendar.StartDate,
                b.Calendar.EndDate,
                Course = new
                {
                    b.Calendar.Course.CourseId,
                    b.Calendar.Course.CourseName
                }
            },
            EnrollmentCount = b.Enrollments.Count, // 👈 actual number of employees
            FeedbackCount = b.Feedbacks.Count
        });

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<Batch>> Get(int id)
    {
        var batch = await _db.Batch
                             .Include(b => b.Calendar)
                                .ThenInclude(c => c.Course)
                             .FirstOrDefaultAsync(b => b.BatchId == id && b.IsActive);

        return batch is null ? NotFound() : batch;
    }

    // Admin + Manager can create
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<Batch>> Create(Batch batch)
    {
        batch.CreatedOn = DateTime.UtcNow;
        batch.IsActive = true;

        _db.Batch.Add(batch);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = batch.BatchId }, batch);
    }

   
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Update(int id, Batch batch)
    {
        if (id != batch.BatchId) return BadRequest();

        var existing = await _db.Batch.FindAsync(id);
        if (existing is null || !existing.IsActive) return NotFound();


        existing.BatchName = batch.BatchName;
        existing.CalendarId = batch.CalendarId;
        existing.ModifiedBy = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        existing.CreatedOn = existing.CreatedOn; 
        existing.IsActive = true; // still active

        await _db.SaveChangesAsync();
        return NoContent();
    }

   
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _db.Batch.FindAsync(id);
        if (batch is null || !batch.IsActive) return NotFound();

        batch.IsActive = false;
        batch.ModifiedBy = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        await _db.SaveChangesAsync();
        return NoContent();
    }
    [HttpGet("inactive")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<Batch>>> Inactive()
    {
        var batches = await _db.Batch
            .Where(b => !b.IsActive)
            .Include(b => b.Calendar)
                .ThenInclude(c => c.Course)
            .AsNoTracking()
            .ToListAsync();

        return Ok(batches);
    }

    [HttpGet("{id:int}/enrollments")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments(int id)
    {
        var enrollments = await _db.Enrollment
            .Where(e => e.BatchId == id)
            .Include(e => e.User)
            .Include(e => e.Manager)
            .Include(e => e.Batch).ThenInclude(b => b.Calendar).ThenInclude(c => c.Course)
            .Select(e => new EnrollmentDto(
                e.EnrollmentId,
                e.User.Username,
                e.Batch.Calendar.Course.CourseName,
                e.Batch.BatchId,
                e.Batch.BatchName,
                e.Status!,
                e.Manager != null ? e.Manager.Username : null
            ))
            .ToListAsync();

        return Ok(enrollments);
    }


}
