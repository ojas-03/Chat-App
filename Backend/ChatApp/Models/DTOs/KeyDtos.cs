namespace ChatApp.Models.DTOs;

public record UploadPrekeyBundleRequest(
    string IdentityPublicKey,
    string SignedPrekey,
    string SignedPrekeySignature,
    int SignedPrekeyId,
    List<OneTimePrekeyDto> OneTimePrekeys
);

public record OneTimePrekeyDto(
    int KeyId,
    string PublicKey
);

public record PrekeyBundleResponse(
    string IdentityPublicKey,
    string SignedPrekey,
    string SignedPrekeySignature,
    int SignedPrekeyId,
    OneTimePrekeyDto? OneTimePrekey
);

public record PrekeyCountResponse(
    int RemainingCount
);
