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

        var grouped = query
            .GroupBy(e => new { e.ContextId, e.ContextType })
            .Select(g => new
            {
                g.Key.ContextId,
                g.Key.ContextType,
                Plays = g.LongCount(e => e.EventType == EventType.PlayStart),
                Likes = g.LongCount(e => e.EventType == EventType.Like),
                Adds = g.LongCount(e => e.EventType == EventType.AddToPlaylist)
            });

        var withScore = grouped
            .Select(g => new
            {
                g.ContextId,
                g.ContextType,
                g.Plays,
                g.Likes,
                g.Adds,
                Score = g.Plays * playWeight + g.Likes * likeWeight + g.Adds * addWeight
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Plays)
            .Take(limit);

        var result = await withScore
            .Select(x => new PopularCollectionResult(
                x.ContextId!.Value,
                x.ContextType,
                x.Plays,
                x.Likes,
                x.Adds,
                x.Score))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<RecentCollectionResult>> GetRecentCollectionsAsync(
        int userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        int? cursorCollectionId,
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
                (x.LastPlayedAtUtc == cursorLastPlayedUtc.Value && x.ContextId!.Value < cursorCollectionId.Value));
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
        int userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        int? cursorTrackId,
        CancellationToken cancellationToken = default)
    {
        // Get all PlayStart events with TrackId for this user
        var query = _dbContext.UserEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.TrackId != null &&
                e.TrackId > 0 &&
                e.EventType == EventType.PlayStart);

        // Get the latest timestamp for each track
        var latestTimestamps = query
            .GroupBy(e => e.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                LastPlayedAtUtc = g.Max(e => e.TimestampUtc)
            });

        // Materialize the latest timestamps first
        var latestList = await latestTimestamps.ToListAsync(cancellationToken);

        if (latestList.Count == 0)
        {
            return new List<RecentTrackResult>();
        }

        // Get all track IDs
        var trackIds = latestList.Select(x => x.TrackId).ToList();
        
        // Get all events for these tracks (we'll filter by timestamp in memory)
        var allEvents = await query
            .Where(e => trackIds.Contains(e.TrackId))
            .ToListAsync(cancellationToken);

        // Create a dictionary for quick lookup of latest timestamp per track
        var latestTimesDict = latestList.ToDictionary(x => x.TrackId!.Value, x => x.LastPlayedAtUtc);

        // Filter events to only those with the latest timestamp for each track
        var matchingEvents = allEvents
            .Where(e => e.TrackId.HasValue && 
                       latestTimesDict.ContainsKey(e.TrackId.Value) &&
                       e.TimestampUtc == latestTimesDict[e.TrackId.Value])
            .ToList();

        // Group by track and take the one with max Id (tiebreaker)
        var perTrack = matchingEvents
            .GroupBy(e => e.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                LastEvent = g.OrderByDescending(e => e.Id).First()
            })
            .Select(x => new
            {
                TrackId = x.TrackId,
                ContextId = x.LastEvent.ContextId,
                ContextType = x.LastEvent.ContextType,
                LastPlayedAtUtc = x.LastEvent.TimestampUtc
            });

        if (cursorLastPlayedUtc.HasValue && cursorTrackId.HasValue)
        {
            perTrack = perTrack.Where(x =>
                x.LastPlayedAtUtc < cursorLastPlayedUtc.Value ||
                (x.LastPlayedAtUtc == cursorLastPlayedUtc.Value && x.TrackId!.Value < cursorTrackId.Value));
        }

        var ordered = perTrack
            .OrderByDescending(x => x.LastPlayedAtUtc)
            .ThenByDescending(x => x.TrackId)
            .Take(limit);

        var result = ordered
            .Select(x => new RecentTrackResult(
                x.TrackId!.Value,
                x.ContextId,
                x.ContextType,
                x.LastPlayedAtUtc))
            .ToList();

        return result;
    }

    public async Task<IReadOnlyList<TopTrackResult>> GetTopTracksAsync(
        int userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.TrackId != null &&
                e.TrackId > 0 &&
                e.EventType == EventType.PlayStart);

        var grouped = query
            .GroupBy(e => e.TrackId)
            .Select(g => new
            {
                TrackId = g.Key,
                PlayCount = g.LongCount()
            })
            .OrderByDescending(x => x.PlayCount)
            .ThenByDescending(x => x.TrackId)
            .Take(limit);

        var result = await grouped
            .Select(x => new TopTrackResult
            {
                TrackId = x.TrackId!.Value,
                PlayCount = x.PlayCount
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}

