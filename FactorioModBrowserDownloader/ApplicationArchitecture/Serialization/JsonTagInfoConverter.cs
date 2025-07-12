using FactorioNexus.ApplicationArchitecture.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Serialization
{
    public class JsonTagInfoConverter : JsonConverter<TagInfo>
    {
        public override TagInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            string? value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                throw new JsonException();

            if (!TagInfo.TryParse(value, out TagInfo? tag))
                tag = new TagInfo(value);

            return tag;
        }

        public override void Write(Utf8JsonWriter writer, TagInfo value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
