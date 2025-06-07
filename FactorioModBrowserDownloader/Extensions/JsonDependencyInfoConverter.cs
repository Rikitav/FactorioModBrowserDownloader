using FactorioModBrowserDownloader.ModPortal.Types;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioModBrowserDownloader.Extensions
{
    public partial class JsonDependencyInfoConverter : JsonConverter<DependencyInfo[]>
    {
        private static readonly Dictionary<string, Dependency> DependancyPrefix = new Dictionary<string, Dependency>()
        {
            { "!", Dependency.Incompatible },
            { "?", Dependency.Optional },
            { "~", Dependency.DontAffect },
            { "(?)", Dependency.Hidden }
        };

        private static readonly Dictionary<string, VersionOperator> DependancyOperators = new Dictionary<string, VersionOperator>()
        {
            { "<", VersionOperator.Less },
            { "<=", VersionOperator.LessOrEqual },
            { "=", VersionOperator.Equal },
            { ">=", VersionOperator.MoreOrEqual },
            { ">", VersionOperator.More }
        };

        private enum State
        {
            Modifier,
            ModId,
            Operator,
            Version,
            End
        }

        public override DependencyInfo[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Regex parser = DependencyParserRegex();
            List<DependencyInfo> dependencies = [];

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                string? value = reader.GetString();
                if (value == null)
                    continue;

                if (string.IsNullOrEmpty(value))
                    continue;

                Match match = parser.Match(value);
                if (!match.Success)
                    continue;

                DependencyInfo dependency = new DependencyInfo();

                if (match.Groups[1].Success && DependancyPrefix.TryGetValue(match.Groups[0].Value, out Dependency prefix))
                    dependency.Prefix = prefix;

                if (match.Groups[1].Success)
                    dependency.ModId = match.Groups[1].Value;

                if (match.Groups[3].Success && DependancyOperators.TryGetValue(match.Groups[2].Value, out VersionOperator versionOperator))
                    dependency.Operator = versionOperator;

                if (match.Groups[4].Success && Version.TryParse(match.Groups[4].Value, out Version? version))
                    dependency.Version = version;

                dependencies.Add(dependency);
            }

            return dependencies.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, DependencyInfo[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        [GeneratedRegex(@"((?:\?|\!|\(\?\)|\~))?\s*(\S+)\s*((?:\<|\<\=|\=|\>\=|\>))?\s*(\S+)?")]
        private static partial Regex DependencyParserRegex();
    }
}
