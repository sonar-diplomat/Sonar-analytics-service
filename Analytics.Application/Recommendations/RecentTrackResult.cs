using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record RecentTrackResult(
    Guid TrackId,
    Guid? ContextId,
    ContextType ContextType,
    DateTime LastPlayedAtUtc);


