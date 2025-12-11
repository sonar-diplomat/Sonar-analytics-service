using Analytics.Application.Recommendations;
using Analytics.Domain;

namespace Analytics.Application.Abstractions;

public interface IUserEventsRepository
{
    Task AddAsync(UserEvent userEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PopularCollectionResult>> GetPopularCollectionsAsync(
        int limit,
        double playWeight,
        double likeWeight,
        double addWeight,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentCollectionResult>> GetRecentCollectionsAsync(
        Guid userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        Guid? cursorCollectionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentTrackResult>> GetRecentTracksAsync(
        Guid userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        Guid? cursorTrackId,
        CancellationToken cancellationToken = default);
}

