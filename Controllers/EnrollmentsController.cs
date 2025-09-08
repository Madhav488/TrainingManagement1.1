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

       
        var dto = new EnrollmentDto(
            enroll.EnrollmentId,
            User.Identity!.Name!,   
            batch.Calendar.Course.CourseName,
            batch.BatchId,
            batch.BatchName,
            enroll.Status!,
            null
        );

        return CreatedAtAction(nameof(GetById), new { id = enroll.EnrollmentId }, dto);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Approve(int id)
    {
        var e = await _db.Enrollment
            .Include(en => en.User)
            .FirstOrDefaultAsync(en => en.EnrollmentId == id);

        if (e == null) return NotFound();

        // 🔹 ensure current manager is assigned manager of employee
        if (e.User.ManagerId != CurrentUserId)
            return Forbid("You are not this employee's manager.");

        e.Status = "Approved";
        e.ManagerId = CurrentUserId;

        await _db.SaveChangesAsync();
        return NoContent();
    }


    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Reject(int id)
    {
        var e = await _db.Enrollment
            .Include(en => en.User)
            .FirstOrDefaultAsync(en => en.EnrollmentId == id);

        if (e == null) return NotFound();

        if (e.User.ManagerId != CurrentUserId)
            return Forbid("You are not this employee's manager.");

        e.Status = "Rejected";
        e.ManagerId = CurrentUserId;

        await _db.SaveChangesAsync();
        return NoContent();
    }


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
                e.Batch.BatchId,
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
                e.Batch.BatchId,
                e.Batch.BatchName,
                e.Status!,
                e.Manager != null ? e.Manager.Username : null
            ))
            .ToListAsync();

        return Ok(result);
    }
    [HttpGet("mine")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> Mine()
    {
        var userId = CurrentUserId;
        var result = await _db.Enrollment
            .Where(e => e.UserId == userId)
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

        return Ok(result);
    }

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
            e.Batch.BatchId,
            e.Batch.BatchName,
            e.Status!,
            e.Manager != null ? e.Manager.Username : null
        );
    }
    [HttpGet("employee/{employeeId:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByEmployee(int employeeId)
    {
        var result = await _db.Enrollment
            .Where(e => e.UserId == employeeId)
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

        return Ok(result);
    }
    [HttpPost("enroll")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EnrollEmployee([FromQuery] int employeeId, [FromQuery] int batchId)
    {
        var exists = await _db.Enrollment.AnyAsync(e => e.UserId == employeeId && e.BatchId == batchId);
        if (exists) return BadRequest("Employee is already enrolled in this batch.");

        var batch = await _db.Batch
            .Include(b => b.Calendar).ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(b => b.BatchId == batchId);

        if (batch == null) return NotFound("Batch not found.");

        var enrollment = new Enrollment
        {
            UserId = employeeId,
            BatchId = batchId,
            Status = "Requested",
            RequestedOn = DateTime.UtcNow
        };

        _db.Enrollment.Add(enrollment);
        await _db.SaveChangesAsync();

        return Ok(new { enrollment.EnrollmentId, enrollment.Status });
    }

    [HttpGet("batch/{batchId:int}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByBatch(int batchId)
    {
        var result = await _db.Enrollment
            .Where(e => e.BatchId == batchId)
            .Include(e => e.User)
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

        return Ok(result);
    }

}
