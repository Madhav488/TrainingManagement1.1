using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tms.Api.Data;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly TmsDbContext _db;
    public EnrollmentsController(TmsDbContext db) => _db = db;

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // Employee: request enrollment
    [HttpPost("request/{batchId:int}")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<Enrollment>> Request(int batchId)
    {
        var exists = await _db.Batch.AnyAsync(b => b.BatchId == batchId);
        if (!exists) return NotFound("Batch not found.");

        var enroll = new Enrollment
        {
            UserId = CurrentUserId,
            BatchId = batchId,
            Status = "Requested",
            RequestedOn = DateTime.UtcNow
        };
        _db.Enrollment.Add(enroll);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = enroll.EnrollmentId }, enroll);
    }

    // Manager: approve
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Approve(int id)
    {
        var e = await _db.Enrollment.FindAsync(id);
        if (e is null) return NotFound();
        e.Status = "Approved";
        e.ManagerId = CurrentUserId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Manager: reject
    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Reject(int id)
    {
        var e = await _db.Enrollment.FindAsync(id);
        if (e is null) return NotFound();
        e.Status = "Rejected";
        e.ManagerId = CurrentUserId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Manager + Admin: pending lists
    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<IEnumerable<Enrollment>>> Pending()
        => await _db.Enrollment.Where(e => e.Status == "Requested")
                               .Include(e => e.User)
                               .Include(e => e.Batch).ThenInclude(b => b.Calendar).ThenInclude(c => c.Course)
                               .AsNoTracking().ToListAsync();

    // Reports for Admin
    [HttpGet("report/requested")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<Enrollment>>> Requested()
        => await _db.Enrollment.Where(e => e.Status == "Requested")
                               .AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Enrollment>> GetById(int id)
        => await _db.Enrollment.Include(x => x.User)
                               .Include(x => x.Manager)
                               .Include(x => x.Batch)
                               .FirstOrDefaultAsync(x => x.EnrollmentId == id)
            is { } e ? e : NotFound();
}
