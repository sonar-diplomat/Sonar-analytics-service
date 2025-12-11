using Analytics.Domain;

namespace Analytics.Application.UserEvents;

public record AddUserEventCommand(
    int UserId,
    int? TrackId,
    EventType EventType,
    ContextType ContextType,
    int? ContextId,
    int? PositionMs,
    int? DurationMs,
    DateTime TimestampUtc,
    string? PayloadJson);

