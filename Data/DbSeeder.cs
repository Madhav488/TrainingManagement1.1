using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Tms.Api.Models;

namespace Tms.Api.Data;

public class DbSeeder
{
    private readonly TmsDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public DbSeeder(TmsDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        // Roles
        if (!await _db.Roles.AnyAsync())
        {
            _db.Roles.AddRange(
                new Role { RoleName = "Administrator" },
                new Role { RoleName = "Manager" },
                new Role { RoleName = "Employee" }
            );
            await _db.SaveChangesAsync();
        }

        // Admin user
        if (!await _db.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRoleId = await _db.Roles.Where(r => r.RoleName == "Administrator")
                                             .Select(r => r.RoleId).FirstAsync();

            var admin = new User
            {
                Username = "admin",
                Email = "admin@abc.com",
                RoleId = adminRoleId,
                CreatedOn = DateTime.UtcNow
            };
            admin.PasswordHash = _hasher.HashPassword(admin, "Admin@123"); // change later

            _db.Users.Add(admin);
            await _db.SaveChangesAsync();
        }
        if (!await _db.Users.AnyAsync(u => u.Username == "manager"))
        {
            var managerRoleId = await _db.Roles.Where(r => r.RoleName == "Manager")
                .Select(r => r.RoleId).FirstAsync();

            var manager = new User
            {
                Username = "manager",
                Email = "manager@abc.com",
                RoleId = managerRoleId,
                CreatedOn = DateTime.UtcNow
            };
            manager.PasswordHash = _hasher.HashPassword(manager, "Manager@123");
            _db.Users.Add(manager);
            await _db.SaveChangesAsync();
        }
    }
}
