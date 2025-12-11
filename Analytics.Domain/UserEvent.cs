namespace Analytics.Domain;

public class UserEvent
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public int? TrackId { get; set; }
    public EventType EventType { get; set; }
    public ContextType ContextType { get; set; }
    public int? ContextId { get; set; }
    public int? PositionMs { get; set; }
    public int? DurationMs { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? PayloadJson { get; set; }
}

