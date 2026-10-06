namespace ChatApp.Models.DTOs;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string ProfilePic,
    DateTime CreatedAt
);
