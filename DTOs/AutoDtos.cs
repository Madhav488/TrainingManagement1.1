namespace Tms.Api.DTOs;

public record RegisterRequest(string Username, string Password, string? Email, string RoleName);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string AccessToken, string Username, string Role);
