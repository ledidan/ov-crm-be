


using System.Text.Json;
using System.Text.Json.Serialization;

namespace Data.Interceptor
{
    public class NullStringConverter : JsonConverter<string?>
    {
        public NullStringConverter() { } // ✅ Ensure a public parameterless constructor

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}