namespace ChatApp.Models.DTOs;

public record SendMessageRequest(
    string? Text,
    string? ImageUrl
);

public record MessageResponse(
    Guid Id,
    Guid SenderId,
    Guid ReceiverId,
    string? Text,
    string? ImageUrl,
    DateTime CreatedAt
);
