using Analytics.Domain;

namespace Analytics.Application.UserEvents;

public record AddUserEventCommand(
    Guid UserId,
    Guid? TrackId,
    EventType EventType,
    ContextType ContextType,
    Guid? ContextId,
    int? PositionMs,
    int? DurationMs,
    DateTime TimestampUtc,
    string? PayloadJson);

