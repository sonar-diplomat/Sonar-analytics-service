using Analytics.Domain;

namespace Analytics.Application.Recommendations;

public record PopularCollectionResult(
    Guid CollectionId,
    ContextType CollectionType,
    long Plays,
    long Likes,
    long Adds,
    double Score);


