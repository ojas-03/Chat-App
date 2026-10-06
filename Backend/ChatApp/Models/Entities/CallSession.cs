namespace ChatApp.Models.Entities;

public class CallSession
{
    public Guid Id { get; set; }
    public Guid CallerId { get; set; }
    public string CallType { get; set; } = "audio";
    public string Status { get; set; } = "initiated";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public User Caller { get; set; } = null!;
    public ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
}
