using Analytics.Application.Abstractions;

namespace Analytics.Application.UserEvents;

public class GetTopArtistsHandler
{
    private readonly IUserEventsRepository _repository;

    public GetTopArtistsHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<TopArtistResult>> HandleAsync(
        GetTopArtistsQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit <= 0 ? 5 : query.Limit;
        return _repository.GetTopArtistsAsync(query.UserId, limit, cancellationToken);
    }
}

