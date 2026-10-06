namespace ChatApp.Services.Interfaces;

using ChatApp.Models.DTOs;

public interface IKeyService
{
    Task UploadBundleAsync(Guid userId, UploadPrekeyBundleRequest request);
    Task<PrekeyBundleResponse?> FetchBundleAsync(Guid targetUserId);
    Task<PrekeyCountResponse> GetPrekeyCountAsync(Guid userId);
}
