namespace Analytics.Application.Recommendations;

public record GetRecentCollectionsQuery(Guid UserId, int Limit, string? Cursor);


