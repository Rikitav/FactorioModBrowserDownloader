using FactorioNexus.ModPortal.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Converters
{
    public class JsonCategoryInfoConverter : JsonConverter<CategoryInfo>
    {
        public override CategoryInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            string? value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return CategoryInfo.Known.ElementAt(0).Value; // "no-category"

            return CategoryInfo.Known[value];
        }

        public override void Write(Utf8JsonWriter writer, CategoryInfo value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
