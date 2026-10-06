namespace ChatApp.Models.Entities;

public class CallParticipant
{
    public Guid Id { get; set; }
    public Guid CallId { get; set; }
    public Guid UserId { get; set; }
    public string ResponseStatus { get; set; } = "ringing";
    public DateTime? JoinedAt { get; set; }

    // Navigation properties
    public CallSession Call { get; set; } = null!;
    public User User { get; set; } = null!;
}
