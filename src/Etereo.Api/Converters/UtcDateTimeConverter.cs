using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etereo.Api.Converters;

/// <summary>
/// Trata todo DateTime entrante desde JSON como UTC.
/// Necesario porque Npgsql 6+ rechaza DateTimeKind.Unspecified en columnas timestamptz.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
}
