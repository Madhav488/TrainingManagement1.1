using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tms.Api.Data;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class CoursesController : ControllerBase
{
    private readonly TmsDbContext _db;
    public CoursesController(TmsDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<IEnumerable<Course>>> GetAll()
    {
        return await _db.Courses
            .Include(c => c.Calendars)
                .ThenInclude(cal => cal.Batches)
            .AsNoTracking()
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<ActionResult<Course>> GetById(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        return course is null ? NotFound() : course;
    }

    [HttpPost]
    public async Task<ActionResult<Course>> Create(Course course)
    {
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, course);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Course updated)
    {
        if (id != updated.CourseId) return BadRequest();
        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null) return NotFound();
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
