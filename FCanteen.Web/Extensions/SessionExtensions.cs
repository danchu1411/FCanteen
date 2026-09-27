using System.Text.Json;

using Microsoft.AspNetCore.Http;

namespace FCanteen.Web.Extensions;

public static class SessionExtensions
{
    private static readonly
        JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

    public static void SetJson<T>(
        this ISession session,
        string key,
        T value)
    {
        var json =
            JsonSerializer.Serialize(
                value,
                JsonOptions);

        session.SetString(
            key,
            json);
    }

    public static T? GetJson<T>(
        this ISession session,
        string key)
    {
        var json =
            session.GetString(
                key);

        if (string.IsNullOrWhiteSpace(
            json))
        {
            return default;
        }

        return JsonSerializer
            .Deserialize<T>(
                json,
                JsonOptions);
    }
}
