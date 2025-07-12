using FactorioNexus.ApplicationArchitecture.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Serialization
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

            if (!CategoryInfo.Known.TryGetValue(value, out CategoryInfo? category))
                category = new CategoryInfo(value, value.FirstLetterToUpper(), value);

            return category;
        }

        public override void Write(Utf8JsonWriter writer, CategoryInfo value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
