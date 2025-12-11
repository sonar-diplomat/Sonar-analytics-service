using Analytics.Application.Abstractions;

namespace Analytics.Application.Recommendations;

public class GetRecentCollectionsHandler
{
    private readonly IUserEventsRepository _repository;

    public GetRecentCollectionsHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<RecentCollectionResult>> HandleAsync(
        GetRecentCollectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit <= 0 ? 5 : query.Limit;
        limit = Math.Min(limit, 50);

        DateTime? cursorTs = null;
        int? cursorId = null;
        if (CursorHelper.TryDecode(query.Cursor, out var ts, out var id))
        {
            cursorTs = ts;
            cursorId = id;
        }

        // Take one extra to detect continuation
        var take = checked(limit + 1);

        return _repository.GetRecentCollectionsAsync(query.UserId, take, cursorTs, cursorId, cancellationToken);
    }
}


