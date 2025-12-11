using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record RecentCollectionResult(
    int CollectionId,
    ContextType CollectionType,
    DateTime LastPlayedAtUtc);


