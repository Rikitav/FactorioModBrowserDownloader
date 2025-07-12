using FactorioNexus.ApplicationArchitecture.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioNexus.ApplicationArchitecture.Serialization
{
    public partial class JsonDependencyInfoConverter : JsonConverter<DependencyInfo>
    {
        public override DependencyInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            string? value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            Match match = DependencyParserRegex().Match(value);
            if (!match.Success)
                throw new FormatException();

            DependencyInfo dependency = new DependencyInfo(match.Groups["modId"].Value);
            if (match.Groups["prefix"].Success && DependencyModifier.TryParse(match.Groups["prefix"].Value, out DependencyModifier? modifier))
                dependency.Modifier = modifier;

            if (match.Groups["operator"].Success && match.Groups["version"].Success)
            {
                if (VersionComparer.TryParse(match.Groups["comparer"].Value, out VersionComparer? versionComparer))
                    dependency.Comparer = versionComparer;

                if (Version.TryParse(match.Groups["version"].Value, out Version? version))
                    dependency.Version = version;
            }

            return dependency;
        }

        public override void Write(Utf8JsonWriter writer, DependencyInfo value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());

        [GeneratedRegex(@"(?:(?'prefix'[?!~]|\(\?\)) )?(?'modId'\w[\w- ]+\w+?)( (?'comparer'<|<=|=|>=|>) (?'version'\S+))?")]
        private static partial Regex DependencyParserRegex();
    }
}
