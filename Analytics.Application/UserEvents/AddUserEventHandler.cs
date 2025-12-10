using Analytics.Application.Abstractions;
using Analytics.Domain;

namespace Analytics.Application.UserEvents;

public class AddUserEventHandler
{
    private readonly IUserEventsRepository _repository;

    public AddUserEventHandler(IUserEventsRepository repository)
    {
        _repository = repository;
    }

    public Task HandleAsync(AddUserEventCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new UserEvent
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            TrackId = command.TrackId,
            EventType = command.EventType,
            ContextType = command.ContextType,
            ContextId = command.ContextId,
            PositionMs = command.PositionMs,
            DurationMs = command.DurationMs,
            TimestampUtc = command.TimestampUtc,
            PayloadJson = command.PayloadJson
        };

        return _repository.AddAsync(entity, cancellationToken);
    }
}

