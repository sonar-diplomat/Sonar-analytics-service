namespace Analytics.Application.Recommendations;

public record GetRecentTracksQuery(int UserId, int Limit, string? Cursor);


