using Analytics.Application.Abstractions;

namespace Analytics.Application.Recommendations;

public class GetPopularCollectionsHandler
{
    private readonly IUserEventsRepository _repository;

    // Weights per user request: plays=1.0, likes=0.5, adds=0.7
    private const double PlayWeight = 1.0;
    private const double LikeWeight = 0.5;
    private const double AddWeight = 0.7;

    public GetPopularCollectionsHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<PopularCollectionResult>> HandleAsync(
        GetPopularCollectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit <= 0 ? 4 : query.Limit;

        return _repository.GetPopularCollectionsAsync(
            limit,
            PlayWeight,
            LikeWeight,
            AddWeight,
            cancellationToken);
    }
}


