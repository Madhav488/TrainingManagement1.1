using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tms.Api.Data;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchesController : ControllerBase
{
    private readonly TmsDbContext _db;
    public BatchesController(TmsDbContext db) => _db = db;

    // All roles can view list and details
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Batch>>> All()
        => await _db.Batch.Include(b => b.Calendar).ThenInclude(c => c.Course)
                          .AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Batch>> Get(int id)
    {
        var batch = await _db.Batch.Include(b => b.Calendar).ThenInclude(c => c.Course)
                                   .FirstOrDefaultAsync(b => b.BatchId == id);
        return batch is null ? NotFound() : batch;
    }

    // Admin CRUD
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<Batch>> Create(Batch batch)
    {
        _db.Batch.Add(batch);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = batch.BatchId }, batch);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(int id, Batch batch)
    {
        if (id != batch.BatchId) return BadRequest();
        _db.Entry(batch).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _db.Batch.FindAsync(id);
        if (batch is null) return NotFound();
        _db.Batch.Remove(batch);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
