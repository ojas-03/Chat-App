namespace ChatApp.Models.Entities;

public class OneTimePrekey
{
    public Guid Id { get; set; }
    public Guid BundleId { get; set; }
    public int KeyId { get; set; }
    public string PublicKey { get; set; } = string.Empty;
    public bool IsConsumed { get; set; } = false;
    public DateTime? ConsumedAt { get; set; }

    // Navigation properties
    public PrekeyBundle Bundle { get; set; } = null!;
}
