


namespace Data.Interceptor
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class NullDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    throw new JsonException($"Invalid value for {typeToConvert}, expected a number but got an empty string.");
                }
                if (double.TryParse(stringValue, out double result))
                {
                    return result;
                }
            }
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out double value))
            {
                return value;
            }
            throw new JsonException($"Invalid value for {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

}