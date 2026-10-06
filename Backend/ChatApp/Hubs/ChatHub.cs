namespace ChatApp.Hubs;

using System.Security.Claims;
using ChatApp.Models.DTOs;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IPresenceTracker _presenceTracker;
    private readonly IMessageService _messageService;

    public ChatHub(IPresenceTracker presenceTracker, IMessageService messageService)
    {
        _presenceTracker = presenceTracker;
        _messageService = messageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var isNewlyOnline = await _presenceTracker.TrackConnectionAsync(userId, Context.ConnectionId);

        // Always send the current online users to the newly connected client
        var onlineUsers = await _presenceTracker.GetOnlineUsersAsync();
        await Clients.Caller.UpdateOnlineUsers(onlineUsers);

        // If user just came online (first device), broadcast to all others
        if (isNewlyOnline)
        {
            await Clients.Others.UpdateOnlineUsers(onlineUsers);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var isNowOffline = await _presenceTracker.UntrackConnectionAsync(userId, Context.ConnectionId);

        // If user went fully offline (last device disconnected), broadcast to all
        if (isNowOffline)
        {
            var onlineUsers = await _presenceTracker.GetOnlineUsersAsync();
            await Clients.All.UpdateOnlineUsers(onlineUsers);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send a message to a specific user. The message is persisted to the database
    /// and delivered in real-time to all active connections of the receiver.
    /// Also echoed back to all sender connections for multi-device sync.
    /// </summary>
    public async Task SendMessage(Guid receiverId, string? text, string? imageUrl)
    {
        var senderId = GetUserId();

        // Persist to database
        var request = new SendMessageRequest(text, imageUrl);
        var messageResponse = await _messageService.SendMessageAsync(senderId, receiverId, request);

        // Send to all receiver connections
        var receiverConnections = await _presenceTracker.GetConnectionsForUserAsync(receiverId);
        foreach (var connectionId in receiverConnections)
        {
            await Clients.Client(connectionId).ReceiveMessage(messageResponse);
        }

        // Echo to all sender connections (for multi-device sync)
        var senderConnections = await _presenceTracker.GetConnectionsForUserAsync(senderId);
        foreach (var connectionId in senderConnections)
        {
            await Clients.Client(connectionId).ReceiveMessage(messageResponse);
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User is not authenticated.");

        return Guid.Parse(userIdClaim);
    }
}
