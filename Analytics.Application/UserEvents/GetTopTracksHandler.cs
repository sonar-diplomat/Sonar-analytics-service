using Analytics.Application.Abstractions;

namespace Analytics.Application.UserEvents;

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
        var limit = query.Limit <= 0 ? 5 : query.Limit;
        return _repository.GetTopTracksAsync(query.UserId, limit, cancellationToken);
    }
}

