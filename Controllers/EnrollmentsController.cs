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
public class EnrollmentsController : ControllerBase
{
    private readonly TmsDbContext _db;
    public EnrollmentsController(TmsDbContext db) => _db = db;

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // 🔹 Employee: request enrollment
    [HttpPost("request/{batchId:int}")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<EnrollmentDto>> Request(int batchId)
    {
        var batch = await _db.Batch
            .Include(b => b.Calendar).ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(b => b.BatchId == batchId);

        if (batch is null) return NotFound("Batch not found.");

        var enroll = new Enrollment
        {
            UserId = CurrentUserId,
            BatchId = batchId,
            Status = "Requested",
            RequestedOn = DateTime.UtcNow
        };

        _db.Enrollment.Add(enroll);
        await _db.SaveChangesAsync();

        // build DTO to return
        var dto = new EnrollmentDto(
            enroll.EnrollmentId,
            User.Identity!.Name!,   // logged-in Employee
            batch.Calendar.Course.CourseName,
            batch.BatchName,
            enroll.Status!,
            null
        );

        return CreatedAtAction(nameof(GetById), new { id = enroll.EnrollmentId }, dto);
    }

    // 🔹 Manager: approve
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

    // 🔹 Manager: reject
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

    // 🔹 Manager + Admin: pending requests
    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> Pending()
    {
        var result = await _db.Enrollment
            .Where(e => e.Status == "Requested")
            .Select(e => new EnrollmentDto(
                e.EnrollmentId,
                e.User.Username,
                e.Batch.Calendar.Course.CourseName,
                e.Batch.BatchName,
                e.Status!,
                e.Manager != null ? e.Manager.Username : null
            ))
            .ToListAsync();

        return Ok(result);
    }

    // 🔹 Reports for Admin
    [HttpGet("report/requested")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> Requested()
    {
        var result = await _db.Enrollment
            .Where(e => e.Status == "Requested")
            .Select(e => new EnrollmentDto(
                e.EnrollmentId,
                e.User.Username,
                e.Batch.Calendar.Course.CourseName,
                e.Batch.BatchName,
                e.Status!,
                e.Manager != null ? e.Manager.Username : null
            ))
            .ToListAsync();

        return Ok(result);
    }

    // 🔹 Shared by all: get single enrollment
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Employee,Manager,Administrator")]
    public async Task<ActionResult<EnrollmentDto>> GetById(int id)
    {
        var e = await _db.Enrollment
            .Include(x => x.User)
            .Include(x => x.Manager)
            .Include(x => x.Batch).ThenInclude(b => b.Calendar).ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(x => x.EnrollmentId == id);

        if (e is null) return NotFound();

        return new EnrollmentDto(
            e.EnrollmentId,
            e.User.Username,
            e.Batch.Calendar.Course.CourseName,
            e.Batch.BatchName,
            e.Status!,
            e.Manager != null ? e.Manager.Username : null
        );
    }
}
