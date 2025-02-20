using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Data.Interceptor
{
    public class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (int.TryParse(stringValue, out int result))
                {
                    return result;
                }
            }
            if (reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }
            throw new JsonException($"Invalid value for {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

