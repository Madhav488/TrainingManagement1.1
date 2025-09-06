using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Tms.Api.Data;
using Tms.Api.Models;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly TmsDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UsersController(TmsDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.RoleName);
        if (role == null) return BadRequest("Invalid role.");

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict("Username already exists.");

        if (role.RoleName == "Employee" && dto.ManagerId == null)
            return BadRequest("Employee must be assigned to a Manager.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,   // 👈 added
            LastName = dto.LastName,
            RoleId = role.RoleId,
            CreatedOn = DateTime.UtcNow,
            ManagerId = role.RoleName == "Employee" ? dto.ManagerId : null
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.UserId, user.Username, role.RoleName });
    }
    [HttpGet("managers")]
    public async Task<ActionResult<IEnumerable<object>>> GetManagers()
    {
        var managers = await _db.Users
            .Where(u => u.Role.RoleName == "Manager")
            .Include(m => m.Employees)
            .Select(m => new {
                UserId = m.UserId,
                Username = m.Username,
                Email = m.Email,
                Employees = m.Employees.Select(e => new { e.UserId, e.Username }).ToList()
            })
            .ToListAsync();

        return Ok(managers);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
