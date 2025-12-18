namespace Analytics.Application.UserEvents;

public record GetTopArtistsQuery(
    int UserId,
    int Limit);

