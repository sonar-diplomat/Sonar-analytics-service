namespace Analytics.Application.Recommendations;

public record GetRecentTracksQuery(Guid UserId, int Limit, string? Cursor);


