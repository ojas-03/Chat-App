namespace ChatApp.Controllers;

using System.Security.Claims;
using ChatApp.Models.DTOs;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ICloudinaryService _cloudinaryService;

    public MessagesController(IMessageService messageService, ICloudinaryService cloudinaryService)
    {
        _messageService = messageService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsersForSidebar()
    {
        var userId = GetUserId();
        var users = await _messageService.GetUsersForSidebarAsync(userId);
        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetMessages(Guid userId)
    {
        var currentUserId = GetUserId();
        var messages = await _messageService.GetMessagesAsync(currentUserId, userId);
        return Ok(messages);
    }

    [HttpPost("send/{receiverId:guid}")]
    public async Task<IActionResult> SendMessage(
        Guid receiverId,
        [FromForm] string? text,
        [FromForm] IFormFile? image)
    {
        var senderId = GetUserId();

        string? imageUrl = null;
        if (image != null)
        {
            imageUrl = await _cloudinaryService.UploadImageAsync(image);
        }

        var request = new SendMessageRequest(text, imageUrl);
        var message = await _messageService.SendMessageAsync(senderId, receiverId, request);
        return Created(string.Empty, message);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(userIdClaim);
    }
}
