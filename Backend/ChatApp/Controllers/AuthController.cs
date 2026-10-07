namespace ChatApp.Controllers;

using System.Security.Claims;
using ChatApp.Models.DTOs;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICloudinaryService _cloudinaryService;

    public AuthController(IAuthService authService, ICloudinaryService cloudinaryService)
    {
        _authService = authService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            SetJwtCookie(response.Token);
            return Created(string.Empty, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            SetJwtCookie(response.Token);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("check")]
    public async Task<IActionResult> Check()
    {
        var userId = GetUserId();
        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] string? fullName,
        [FromForm] IFormFile? profilePic)
    {
        try
        {
            var userId = GetUserId();

            string? profilePicUrl = null;
            if (profilePic != null && profilePic.Length > 0)
            {
                profilePicUrl = await _cloudinaryService.UploadImageAsync(profilePic);
            }

            var request = new UpdateProfileRequest(fullName, profilePicUrl);
            var profile = await _authService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(userIdClaim);
    }

    private void SetJwtCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };

        Response.Cookies.Append("jwt", token, cookieOptions);
    }
}
