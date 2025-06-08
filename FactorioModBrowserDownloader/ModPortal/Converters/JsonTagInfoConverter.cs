using FactorioNexus.ModPortal.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Converters
{
    public class JsonTagInfoConverter : JsonConverter<TagInfo>
    {
        public override TagInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            string? value = reader.GetString();
            if (value == null)
                throw new JsonException();

            return TagInfo.Known[value];
        }

        public override void Write(Utf8JsonWriter writer, TagInfo value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
