using FactorioNexus.ModPortal.Types;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioNexus.ModPortal.Converters
{
    public class JsonTagInfoConverter : JsonConverter<TagInfo[]>
    {
        public override TagInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<TagInfo> tags = [];
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                string? value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    throw new JsonException();

                tags.Add(TagInfo.Known[value]);
            }

            return tags.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, TagInfo[] value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
