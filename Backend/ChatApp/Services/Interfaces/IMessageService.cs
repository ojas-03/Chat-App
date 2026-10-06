namespace ChatApp.Services.Interfaces;

using ChatApp.Models.DTOs;

public interface IMessageService
{
    Task<MessageResponse> SendMessageAsync(Guid senderId, Guid receiverId, SendMessageRequest request);
    Task<List<MessageResponse>> GetMessagesAsync(Guid userId1, Guid userId2);
    Task<List<UserProfileResponse>> GetUsersForSidebarAsync(Guid currentUserId);
}
