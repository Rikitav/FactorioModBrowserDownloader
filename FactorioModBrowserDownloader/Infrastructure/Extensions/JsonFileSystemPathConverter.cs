using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioNexus.ApplicationArchitecture.Serialization
{
    public partial class JsonFileSystemPathConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            string? value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return value;

            return EnvVarReaderRegex().Replace(value, VarConverter);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);

        private static string VarConverter(Match match)
            => Environment.GetEnvironmentVariable(match.Groups["var"].Value) ?? match.Value;

        [GeneratedRegex(@"\%(?'var'\S+?)\%")]
        private partial Regex EnvVarReaderRegex();
    }
}
