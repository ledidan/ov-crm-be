using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Data.Interceptor
{
    public class NullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    return null; // Convert empty strings to null
                }
                if (bool.TryParse(stringValue, out bool result))
                {
                    return result;
                }
            }
            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                return reader.GetBoolean();
            }
            throw new JsonException($"Invalid value for {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteBooleanValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

