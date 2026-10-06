namespace ChatApp.Hubs;

using ChatApp.Models.DTOs;

public interface IChatClient
{
    Task ReceiveMessage(MessageResponse message);
    Task UpdateOnlineUsers(List<Guid> onlineUserIds);
}
