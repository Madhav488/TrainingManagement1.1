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

    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<object>>> GetEmployees()
    {
        var employees = await _db.Users
            .Where(u => u.Role.RoleName == "Employee")
            .Select(e => new {
                e.UserId,
                e.Username,
                e.FirstName,
                e.LastName,
                Manager = e.Manager == null ? null : new { e.Manager.UserId, e.Manager.Username }
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignEmployee([FromQuery] int employeeId, [FromQuery] int managerId)
    {
        var employee = await _db.Users.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        employee.ManagerId = managerId;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("unassign/{employeeId}")]
    public async Task<IActionResult> UnassignEmployee(int employeeId)
    {
        var employee = await _db.Users.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        employee.ManagerId = null;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("employee/{id}")]
    public async Task<ActionResult<object>> GetEmployeeById(int id)
    {
        var e = await _db.Users
            .Where(u => u.UserId == id && u.Role.RoleName == "Employee")
            .Select(emp => new {
                emp.UserId,
                emp.Username,
                emp.FirstName,
                emp.LastName,
                Manager = emp.Manager == null ? null : new { emp.Manager.UserId, emp.Manager.Username }
            })
            .FirstOrDefaultAsync();

        if (e == null) return NotFound();
        return Ok(e);
    }

}
