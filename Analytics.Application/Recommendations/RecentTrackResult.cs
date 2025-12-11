using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record RecentTrackResult(
    int TrackId,
    int? ContextId,
    ContextType ContextType,
    DateTime LastPlayedAtUtc);


