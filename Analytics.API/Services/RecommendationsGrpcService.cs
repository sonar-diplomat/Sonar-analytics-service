using Analytics.API;
using Analytics.Application.Recommendations;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using DomainContextType = Analytics.Domain.ContextType;

namespace Analytics.API.Services;

public class RecommendationsGrpcService : Recommendations.RecommendationsBase
{
    private readonly GetPopularCollectionsHandler _popularHandler;
    private readonly GetRecentCollectionsHandler _recentCollectionsHandler;
    private readonly GetRecentTracksHandler _recentTracksHandler;

    public RecommendationsGrpcService(
        GetPopularCollectionsHandler popularHandler,
        GetRecentCollectionsHandler recentCollectionsHandler,
        GetRecentTracksHandler recentTracksHandler)
    {
        _popularHandler = popularHandler;
        _recentCollectionsHandler = recentCollectionsHandler;
        _recentTracksHandler = recentTracksHandler;
    }

    public override async Task<GetPopularCollectionsResponse> GetPopularCollections(
        GetPopularCollectionsRequest request,
        ServerCallContext context)
    {
        var results = await _popularHandler.HandleAsync(
            new GetPopularCollectionsQuery(request.Limit),
            context.CancellationToken);

        var response = new GetPopularCollectionsResponse();
        response.Collections.AddRange(results.Select(r => new PopularCollection
        {
            CollectionId = r.CollectionId,
            CollectionType = MapCollectionType(r.CollectionType),
            Score = r.Score,
            Plays = r.Plays,
            Likes = r.Likes,
            Adds = r.Adds
        }));

        return response;
    }

    public override async Task<GetRecentCollectionsResponse> GetRecentCollections(
        GetRecentCollectionsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit <= 0 ? 5 : request.Limit;
        limit = Math.Min(limit, 50);

        var results = await _recentCollectionsHandler.HandleAsync(
            new GetRecentCollectionsQuery(request.UserId, limit, request.Cursor),
            context.CancellationToken);

        var hasMore = results.Count > limit;
        var page = results.Take(limit).ToList();

        var response = new GetRecentCollectionsResponse();
        response.Collections.AddRange(page.Select(r => new RecentCollection
        {
            CollectionId = r.CollectionId,
            CollectionType = MapCollectionType(r.CollectionType),
            LastPlayedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(r.LastPlayedAtUtc, DateTimeKind.Utc))
        }));

        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            response.NextCursor = CursorHelper.Encode(last.LastPlayedAtUtc, last.CollectionId);
        }

        return response;
    }

    public override async Task<GetRecentTracksResponse> GetRecentTracks(
        GetRecentTracksRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit <= 0 ? 5 : request.Limit;
        limit = Math.Min(limit, 50);

        var results = await _recentTracksHandler.HandleAsync(
            new GetRecentTracksQuery(request.UserId, limit, request.Cursor),
            context.CancellationToken);

        var hasMore = results.Count > limit;
        var page = results.Take(limit).ToList();

        var response = new GetRecentTracksResponse();
        response.Tracks.AddRange(page.Select(r => new RecentTrack
        {
            TrackId = r.TrackId,
            LastPlayedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(r.LastPlayedAtUtc, DateTimeKind.Utc)),
            ContextId = r.ContextId ?? 0,
            ContextType = MapContextType(r.ContextType)
        }));

        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            response.NextCursor = CursorHelper.Encode(last.LastPlayedAtUtc, last.TrackId);
        }

        return response;
    }

    private static CollectionType MapCollectionType(DomainContextType domain) =>
        domain switch
        {
            DomainContextType.ContextAlbum => CollectionType.CollectionAlbum,
            DomainContextType.ContextPlaylist => CollectionType.CollectionPlaylist,
            _ => CollectionType.CollectionUnknown
        };

    private static API.ContextType MapContextType(DomainContextType domain) =>
        domain switch
        {
            DomainContextType.ContextTrack => API.ContextType.ContextTrack,
            DomainContextType.ContextPlaylist => API.ContextType.ContextPlaylist,
            DomainContextType.ContextAlbum => API.ContextType.ContextAlbum,
            DomainContextType.ContextRadio => API.ContextType.ContextRadio,
            DomainContextType.ContextSearch => API.ContextType.ContextSearch,
            _ => API.ContextType.ContextUnknown
        };
}


