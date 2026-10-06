namespace ChatApp.Services;

using ChatApp.Data;
using ChatApp.Models.DTOs;
using ChatApp.Models.Entities;
using ChatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MessageResponse> SendMessageAsync(Guid senderId, Guid receiverId, SendMessageRequest request)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = request.Text,
            ImageUrl = request.ImageUrl
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return new MessageResponse(
            message.Id,
            message.SenderId,
            message.ReceiverId,
            message.Text,
            message.ImageUrl,
            message.CreatedAt
        );
    }

    public async Task<List<MessageResponse>> GetMessagesAsync(Guid userId1, Guid userId2)
    {
        var messages = await _context.Messages
            .Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(
                m.Id,
                m.SenderId,
                m.ReceiverId,
                m.Text,
                m.ImageUrl,
                m.CreatedAt
            ))
            .ToListAsync();

        return messages;
    }

    public async Task<List<UserProfileResponse>> GetUsersForSidebarAsync(Guid currentUserId)
    {
        var users = await _context.Users
            .Where(u => u.Id != currentUserId)
            .OrderBy(u => u.FullName)
            .Select(u => new UserProfileResponse(
                u.Id,
                u.Email,
                u.FullName,
                u.ProfilePic,
                u.CreatedAt
            ))
            .ToListAsync();

        return users;
    }
}
