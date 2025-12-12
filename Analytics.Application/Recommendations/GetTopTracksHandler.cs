using Analytics.Application.Abstractions;

namespace Analytics.Application.Recommendations;

public class GetTopTracksHandler
{
    private readonly IUserEventsRepository _repository;

    public GetTopTracksHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<TopTrackResult>> HandleAsync(
        GetTopTracksQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit <= 0 ? 10 : query.Limit;
        limit = Math.Min(limit, 100);

        return _repository.GetTopTracksAsync(query.UserId, limit, cancellationToken);
    }
}

