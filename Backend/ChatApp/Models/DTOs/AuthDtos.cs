namespace ChatApp.Models.DTOs;

public record RegisterRequest(
    string Email,
    string FullName,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record UpdateProfileRequest(
    string? FullName,
    string? ProfilePic
);

public record AuthResponse(
    Guid Id,
    string Email,
    string FullName,
    string ProfilePic,
    string Token
);
