namespace Tms.Api.DTOs;

// Removed RoleName from RegisterRequest
public record RegisterRequest(string Username, string Password, string? Email);

public record LoginRequest(string Username, string Password);

public record AuthResponse(string AccessToken, int UserId, string Username, string Role);
