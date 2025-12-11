using Analytics.Application.Abstractions;
using Analytics.Application.Recommendations;
using Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public class UserEventsRepository : IUserEventsRepository
{
    private readonly AnalyticsDbContext _dbContext;

    public UserEventsRepository(AnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserEvent userEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserEvents.AddAsync(userEvent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PopularCollectionResult>> GetPopularCollectionsAsync(
        int limit,
        double playWeight,
        double likeWeight,
        double addWeight,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserEvents
            .AsNoTracking()
            .Where(e =>
                e.ContextId != null &&
                (e.ContextType == ContextType.ContextAlbum || e.ContextType == ContextType.ContextPlaylist));

        var result = await query
            .GroupBy(e => new { e.ContextId, e.ContextType })
            .Select(g => new
            {
                g.Key.ContextId,
                g.Key.ContextType,
                Plays = g.LongCount(e => e.EventType == EventType.PlayStart),
                Likes = g.LongCount(e => e.EventType == EventType.Like),
                Adds = g.LongCount(e => e.EventType == EventType.AddToPlaylist)
            })
            .Select(g => new PopularCollectionResult(
                g.ContextId!.Value,
                g.ContextType,
                g.Plays,
                g.Likes,
                g.Adds,
                g.Plays * playWeight + g.Likes * likeWeight + g.Adds * addWeight))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Plays)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<RecentCollectionResult>> GetRecentCollectionsAsync(
        Guid userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        Guid? cursorCollectionId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.ContextId != null &&
                (e.ContextType == ContextType.ContextAlbum || e.ContextType == ContextType.ContextPlaylist) &&
                e.EventType == EventType.PlayStart);

        var grouped = query
            .GroupBy(e => new { e.ContextId, e.ContextType })
            .Select(g => new
            {
                g.Key.ContextId,
                g.Key.ContextType,
                LastPlayedAtUtc = g.Max(e => e.TimestampUtc)
            });

        if (cursorLastPlayedUtc.HasValue && cursorCollectionId.HasValue)
        {
            grouped = grouped.Where(x =>
                x.LastPlayedAtUtc < cursorLastPlayedUtc.Value ||
                (x.LastPlayedAtUtc == cursorLastPlayedUtc.Value && x.ContextId!.Value.CompareTo(cursorCollectionId.Value) < 0));
        }

        var result = await grouped
            .OrderByDescending(x => x.LastPlayedAtUtc)
            .ThenByDescending(x => x.ContextId)
            .Take(limit)
            .Select(x => new RecentCollectionResult(
                x.ContextId!.Value,
                x.ContextType,
                x.LastPlayedAtUtc))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<RecentTrackResult>> GetRecentTracksAsync(
        Guid userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        Guid? cursorTrackId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.TrackId != null &&
                e.EventType == EventType.PlayStart);

        var perTrack = query
            .GroupBy(e => e.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                LastEvent = g.OrderByDescending(e => e.TimestampUtc).First()
            });

        if (cursorLastPlayedUtc.HasValue && cursorTrackId.HasValue)
        {
            perTrack = perTrack.Where(x =>
                x.LastEvent.TimestampUtc < cursorLastPlayedUtc.Value ||
                (x.LastEvent.TimestampUtc == cursorLastPlayedUtc.Value && x.TrackId!.Value.CompareTo(cursorTrackId.Value) < 0));
        }

        var result = await perTrack
            .OrderByDescending(x => x.LastEvent.TimestampUtc)
            .ThenByDescending(x => x.TrackId)
            .Take(limit)
            .Select(x => new RecentTrackResult(
                x.TrackId!.Value,
                x.LastEvent.ContextId,
                x.LastEvent.ContextType,
                x.LastEvent.TimestampUtc))
            .ToListAsync(cancellationToken);

        return result;
    }
}

