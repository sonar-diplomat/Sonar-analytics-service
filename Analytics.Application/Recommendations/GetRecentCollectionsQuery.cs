namespace Analytics.Application.Recommendations;

public record GetRecentCollectionsQuery(int UserId, int Limit, string? Cursor);


