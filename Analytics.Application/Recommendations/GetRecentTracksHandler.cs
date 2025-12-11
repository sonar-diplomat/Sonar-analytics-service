using Analytics.Application.Abstractions;

namespace Analytics.Application.Recommendations;

public class GetRecentTracksHandler
{
    private readonly IUserEventsRepository _repository;

    public GetRecentTracksHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<RecentTrackResult>> HandleAsync(
        GetRecentTracksQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit <= 0 ? 5 : query.Limit;
        limit = Math.Min(limit, 50);

        DateTime? cursorTs = null;
        Guid? cursorId = null;
        if (CursorHelper.TryDecode(query.Cursor, out var ts, out var id))
        {
            cursorTs = ts;
            cursorId = id;
        }

        var take = checked(limit + 1);

        return _repository.GetRecentTracksAsync(query.UserId, take, cursorTs, cursorId, cancellationToken);
    }
}


