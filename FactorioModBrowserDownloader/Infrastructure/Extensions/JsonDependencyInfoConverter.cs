using FactorioNexus.Infrastructure.Models;
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
            if (match.Groups["prefix"].Success && DependencyInfo.CompatibilityTag.TryParse(match.Groups["prefix"].Value, out DependencyInfo.CompatibilityTag? modifier))
                dependency.Modifier = modifier.Value;

            if (match.Groups["operator"].Success && match.Groups["version"].Success)
            {
                if (DependencyInfo.VersionComparer.TryParse(match.Groups["comparer"].Value, out DependencyInfo.VersionComparer? versionComparer))
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
