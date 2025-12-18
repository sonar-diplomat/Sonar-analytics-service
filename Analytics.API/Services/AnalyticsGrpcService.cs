using Analytics.Application.UserEvents;
using Grpc.Core;

namespace Analytics.API.Services;

public class AnalyticsGrpcService : Analytics.AnalyticsBase
{
    private readonly AddUserEventHandler _handler;
    private readonly Application.UserEvents.GetTopTracksHandler _topTracksHandler;
    private readonly GetTopArtistsHandler _topArtistsHandler;
    private readonly ILogger<AnalyticsGrpcService> _logger;

    public AnalyticsGrpcService(
        AddUserEventHandler handler,
        Application.UserEvents.GetTopTracksHandler topTracksHandler,
        GetTopArtistsHandler topArtistsHandler,
        ILogger<AnalyticsGrpcService> logger)
    {
        _handler = handler;
        _topTracksHandler = topTracksHandler;
        _topArtistsHandler = topArtistsHandler;
        _logger = logger;
    }

    public override async Task<AddUserEventResponse> AddUserEvent(UserEventRequest request, ServerCallContext context)
    {
        var command = new AddUserEventCommand(
            request.UserId,
            request.TrackId == 0 ? null : request.TrackId,
            MapEventType(request.EventType),
            MapContextType(request.ContextType),
            request.ContextId == 0 ? null : request.ContextId,
            request.PositionMs == 0 ? null : checked((int)request.PositionMs),
            request.DurationMs == 0 ? null : checked((int)request.DurationMs),
            request.Timestamp.ToDateTime().ToUniversalTime(),
            string.IsNullOrWhiteSpace(request.Payload) ? null : request.Payload);

        await _handler.HandleAsync(command, context.CancellationToken);

        return new AddUserEventResponse { Success = true };
    }

    private static Domain.EventType MapEventType(API.EventType proto) =>
        proto switch
        {
            API.EventType.PlayStart => Domain.EventType.PlayStart,
            API.EventType.PlayFinish => Domain.EventType.PlayFinish,
            API.EventType.Skip => Domain.EventType.Skip,
            API.EventType.Like => Domain.EventType.Like,
            API.EventType.AddToPlaylist => Domain.EventType.AddToPlaylist,
            API.EventType.Search => Domain.EventType.Search,
            _ => Domain.EventType.EventUnknown
        };

    public override async Task<GetTopTracksResponse> GetTopTracks(
        GetTopTracksRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit <= 0 ? 5 : request.Limit;

        var results = await _topTracksHandler.HandleAsync(
            new GetTopTracksQuery(request.UserId, limit),
            context.CancellationToken);

        var response = new GetTopTracksResponse();
        response.Tracks.AddRange(results.Select(r => new TopTrack
        {
            TrackId = r.TrackId,
            PlayCount = r.PlayCount
        }));

        return response;
    }

    public override async Task<GetTopArtistsResponse> GetTopArtists(
        GetTopArtistsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit <= 0 ? 5 : request.Limit;

        var results = await _topArtistsHandler.HandleAsync(
            new GetTopArtistsQuery(request.UserId, limit),
            context.CancellationToken);

        var response = new GetTopArtistsResponse();
        response.Artists.AddRange(results.Select(r => new TopArtist
        {
            ArtistId = r.ArtistId,
            PlayCount = r.PlayCount
        }));

        return response;
    }

    private static Domain.ContextType MapContextType(API.ContextType proto) =>
        proto switch
        {
            API.ContextType.ContextTrack => Domain.ContextType.ContextTrack,
            API.ContextType.ContextPlaylist => Domain.ContextType.ContextPlaylist,
            API.ContextType.ContextAlbum => Domain.ContextType.ContextAlbum,
            API.ContextType.ContextRadio => Domain.ContextType.ContextRadio,
            API.ContextType.ContextSearch => Domain.ContextType.ContextSearch,
            _ => Domain.ContextType.ContextUnknown
        };
}

