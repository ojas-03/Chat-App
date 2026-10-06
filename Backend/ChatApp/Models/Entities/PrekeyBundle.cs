namespace ChatApp.Models.Entities;

public class PrekeyBundle
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IdentityPublicKey { get; set; } = string.Empty;
    public string SignedPrekey { get; set; } = string.Empty;
    public string SignedPrekeySignature { get; set; } = string.Empty;
    public int SignedPrekeyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OneTimePrekey> OneTimePrekeys { get; set; } = new List<OneTimePrekey>();
}
