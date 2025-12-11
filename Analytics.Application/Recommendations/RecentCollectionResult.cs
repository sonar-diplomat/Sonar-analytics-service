using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record RecentCollectionResult(
    Guid CollectionId,
    ContextType CollectionType,
    DateTime LastPlayedAtUtc);


