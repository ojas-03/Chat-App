namespace ChatApp.Services.Interfaces;

using Microsoft.AspNetCore.Http;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string publicId);
}
