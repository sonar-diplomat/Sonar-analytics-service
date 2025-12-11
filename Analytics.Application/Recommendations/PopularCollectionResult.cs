using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record PopularCollectionResult(
    int CollectionId,
    ContextType CollectionType,
    long Plays,
    long Likes,
    long Adds,
    double Score);


