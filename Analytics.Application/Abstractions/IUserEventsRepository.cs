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
        int userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        int? cursorCollectionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentTrackResult>> GetRecentTracksAsync(
        int userId,
        int limit,
        DateTime? cursorLastPlayedUtc,
        int? cursorTrackId,
        CancellationToken cancellationToken = default);
}

