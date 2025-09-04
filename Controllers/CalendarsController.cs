using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tms.Api.Data;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarsController : ControllerBase
{
    private readonly TmsDbContext _db;
    public CalendarsController(TmsDbContext db) => _db = db;

    
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<IEnumerable<CourseCalendar>>> All()
        => await _db.CourseCalendar.AsNoTracking().ToListAsync();

    // Admin CRUD
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<CourseCalendar>> Create(CourseCalendar cal)
    {
        _db.CourseCalendar.Add(cal);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = cal.CalendarId }, cal);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<CourseCalendar>> Get(int id)
        => await _db.CourseCalendar.FindAsync(id) is { } c ? c : NotFound();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(int id, CourseCalendar cal)
    {
        if (id != cal.CalendarId) return BadRequest();
        _db.Entry(cal).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var cal = await _db.CourseCalendar.FindAsync(id);
        if (cal is null) return NotFound();
        _db.CourseCalendar.Remove(cal);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
