using System.Text;
using System.Globalization;

namespace Analytics.Application.Recommendations;

public static class CursorHelper
{
    private const char Separator = '|';

    public static bool TryDecode(string? cursor, out DateTime lastPlayedUtc, out Guid id)
    {
        lastPlayedUtc = default;
        id = default;

        if (string.IsNullOrWhiteSpace(cursor))
            return false;

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        }
        catch
        {
            return false;
        }

        var parts = decoded.Split(Separator);
        if (parts.Length != 2)
            return false;

        if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
            return false;

        if (!Guid.TryParse(parts[1], out id))
            return false;

        lastPlayedUtc = new DateTime(ticks, DateTimeKind.Utc);
        return true;
    }

    public static string Encode(DateTime lastPlayedUtc, Guid id)
    {
        var payload = string.Create(CultureInfo.InvariantCulture, $"{lastPlayedUtc.Ticks}{Separator}{id}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }
}


