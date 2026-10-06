namespace ChatApp.Services;

using ChatApp.Data;
using ChatApp.Models.DTOs;
using ChatApp.Models.Entities;
using ChatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class KeyService : IKeyService
{
    private readonly AppDbContext _context;

    public KeyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task UploadBundleAsync(Guid userId, UploadPrekeyBundleRequest request)
    {
        // Check if user already has a bundle — upsert
        var existingBundle = await _context.PrekeyBundles
            .Include(b => b.OneTimePrekeys)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (existingBundle != null)
        {
            // Update existing bundle
            existingBundle.IdentityPublicKey = request.IdentityPublicKey;
            existingBundle.SignedPrekey = request.SignedPrekey;
            existingBundle.SignedPrekeySignature = request.SignedPrekeySignature;
            existingBundle.SignedPrekeyId = request.SignedPrekeyId;

            // Add new one-time prekeys (don't remove existing unconsumed ones)
            foreach (var otp in request.OneTimePrekeys)
            {
                existingBundle.OneTimePrekeys.Add(new OneTimePrekey
                {
                    BundleId = existingBundle.Id,
                    KeyId = otp.KeyId,
                    PublicKey = otp.PublicKey,
                    IsConsumed = false
                });
            }
        }
        else
        {
            // Create new bundle
            var bundle = new PrekeyBundle
            {
                UserId = userId,
                IdentityPublicKey = request.IdentityPublicKey,
                SignedPrekey = request.SignedPrekey,
                SignedPrekeySignature = request.SignedPrekeySignature,
                SignedPrekeyId = request.SignedPrekeyId
            };

            foreach (var otp in request.OneTimePrekeys)
            {
                bundle.OneTimePrekeys.Add(new OneTimePrekey
                {
                    KeyId = otp.KeyId,
                    PublicKey = otp.PublicKey,
                    IsConsumed = false
                });
            }

            _context.PrekeyBundles.Add(bundle);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<PrekeyBundleResponse?> FetchBundleAsync(Guid targetUserId)
    {
        var bundle = await _context.PrekeyBundles
            .Include(b => b.OneTimePrekeys.Where(o => !o.IsConsumed))
            .FirstOrDefaultAsync(b => b.UserId == targetUserId);

        if (bundle == null)
            return null;

        // Atomically consume one OTP
        OneTimePrekeyDto? consumedOtp = null;
        var availableOtp = bundle.OneTimePrekeys.FirstOrDefault();

        if (availableOtp != null)
        {
            availableOtp.IsConsumed = true;
            availableOtp.ConsumedAt = DateTime.UtcNow;
            consumedOtp = new OneTimePrekeyDto(availableOtp.KeyId, availableOtp.PublicKey);
            await _context.SaveChangesAsync();
        }

        return new PrekeyBundleResponse(
            bundle.IdentityPublicKey,
            bundle.SignedPrekey,
            bundle.SignedPrekeySignature,
            bundle.SignedPrekeyId,
            consumedOtp
        );
    }

    public async Task<PrekeyCountResponse> GetPrekeyCountAsync(Guid userId)
    {
        var bundle = await _context.PrekeyBundles
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (bundle == null)
            return new PrekeyCountResponse(0);

        var count = await _context.OneTimePrekeys
            .CountAsync(o => o.BundleId == bundle.Id && !o.IsConsumed);

        return new PrekeyCountResponse(count);
    }
}
