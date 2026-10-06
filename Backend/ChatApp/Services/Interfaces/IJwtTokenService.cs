namespace ChatApp.Services.Interfaces;

using ChatApp.Models.Entities;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
