


using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Data.Interceptor {
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly string _dateFormat = "yyyy-MM-ddTHH:mm:ss";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
            if (DateTime.TryParseExact(stringValue, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
        }
        if (reader.TryGetDateTime(out DateTime parsedDate))
        {
            return parsedDate;
        }
        throw new JsonException($"Invalid value for {typeToConvert}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(_dateFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

}
