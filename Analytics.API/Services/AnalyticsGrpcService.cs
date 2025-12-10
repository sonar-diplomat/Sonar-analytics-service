using Analytics.API;
using Analytics.Application.UserEvents;
using Analytics.Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Analytics.API.Services;

public class AnalyticsGrpcService : Analytics.AnalyticsBase
{
    private readonly AddUserEventHandler _handler;
    private readonly ILogger<AnalyticsGrpcService> _logger;

    public AnalyticsGrpcService(AddUserEventHandler handler, ILogger<AnalyticsGrpcService> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public override async Task<AddUserEventResponse> AddUserEvent(UserEventRequest request, ServerCallContext context)
    {
        var command = new AddUserEventCommand(
            Guid.Parse(request.UserId),
            ParseGuidOrNull(request.TrackId),
            MapEventType(request.EventType),
            MapContextType(request.ContextType),
            ParseGuidOrNull(request.ContextId),
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

    private static Guid? ParseGuidOrNull(string value) =>
        Guid.TryParse(value, out var guid) ? guid : null;
}

