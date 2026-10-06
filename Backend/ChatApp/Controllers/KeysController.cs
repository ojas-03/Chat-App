namespace ChatApp.Controllers;

using System.Security.Claims;
using ChatApp.Models.DTOs;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KeysController : ControllerBase
{
    private readonly IKeyService _keyService;

    public KeysController(IKeyService keyService)
    {
        _keyService = keyService;
    }

    /// <summary>
    /// Upload identity key, signed prekey, and a batch of one-time prekeys.
    /// </summary>
    [HttpPost("upload-bundle")]
    public async Task<IActionResult> UploadBundle([FromBody] UploadPrekeyBundleRequest request)
    {
        var userId = GetUserId();
        await _keyService.UploadBundleAsync(userId, request);
        return Ok(new { message = "Prekey bundle uploaded successfully." });
    }

    /// <summary>
    /// Fetch a user's prekey bundle for initiating an E2EE session (X3DH).
    /// Atomically consumes one unused one-time prekey.
    /// </summary>
    [HttpGet("bundle/{userId:guid}")]
    public async Task<IActionResult> FetchBundle(Guid userId)
    {
        var bundle = await _keyService.FetchBundleAsync(userId);

        if (bundle == null)
            return NotFound(new { message = "No prekey bundle found for this user." });

        return Ok(bundle);
    }

    /// <summary>
    /// Get the count of remaining unused one-time prekeys for the current user.
    /// Clients should replenish when this count drops below a threshold.
    /// </summary>
    [HttpGet("prekey-count")]
    public async Task<IActionResult> GetPrekeyCount()
    {
        var userId = GetUserId();
        var count = await _keyService.GetPrekeyCountAsync(userId);
        return Ok(count);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(userIdClaim);
    }
}
