public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string RoleName { get; set; } = null!;
    public int? ManagerId { get; set; }
}
