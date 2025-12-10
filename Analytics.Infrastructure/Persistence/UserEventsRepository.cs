using Analytics.Application.Abstractions;
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
}

