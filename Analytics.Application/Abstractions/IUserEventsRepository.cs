using Analytics.Domain;

namespace Analytics.Application.Abstractions;

public interface IUserEventsRepository
{
    Task AddAsync(UserEvent userEvent, CancellationToken cancellationToken = default);
}

