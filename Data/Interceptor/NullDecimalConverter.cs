
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Data.Interceptor
{
    public class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (decimal.TryParse(stringValue, out decimal result))
                {
                    return result;
                }
            }
            if (reader.TryGetDecimal(out decimal decimalValue))
            {
                return decimalValue;
            }
            throw new JsonException($"Invalid value for {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
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