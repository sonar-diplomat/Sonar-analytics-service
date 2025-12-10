namespace Analytics.Domain;

public class UserEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TrackId { get; set; }
    public EventType EventType { get; set; }
    public ContextType ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public int? PositionMs { get; set; }
    public int? DurationMs { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? PayloadJson { get; set; }
}

