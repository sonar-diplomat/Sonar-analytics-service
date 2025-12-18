namespace Analytics.Application.UserEvents;

public record GetTopTracksQuery(
    int UserId,
    int Limit);

