using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tms.Api.Data;
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
    public async Task<ActionResult<IEnumerable<Batch>>> All()
        => await _db.Batch
                    .Where(b => b.IsActive) // only active
                    .Include(b => b.Calendar)
                        .ThenInclude(c => c.Course)
                    .AsNoTracking()
                    .ToListAsync();

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

    // Admin + Manager can update
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Update(int id, Batch batch)
    {
        if (id != batch.BatchId) return BadRequest();

        var existing = await _db.Batch.FindAsync(id);
        if (existing is null || !existing.IsActive) return NotFound();

        // Update properties
        existing.BatchName = batch.BatchName;
        existing.CalendarId = batch.CalendarId;
        existing.ModifiedBy = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        existing.CreatedOn = existing.CreatedOn; // preserve original
        existing.IsActive = true; // still active

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Only Admin can soft delete
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
}
