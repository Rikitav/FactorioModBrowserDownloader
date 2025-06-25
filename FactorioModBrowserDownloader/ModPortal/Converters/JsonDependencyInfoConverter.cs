using FactorioNexus.ModPortal.Types;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioNexus.ModPortal.Converters
{
    public partial class JsonDependencyInfoConverter : JsonConverter<DependencyInfo[]>
    {
        private static readonly Dictionary<string, DependencyModifier> DependencyPrefix = new Dictionary<string, DependencyModifier>()
        {
            { "!", DependencyModifier.Incompatible },
            { "?", DependencyModifier.Optional },
            { "~", DependencyModifier.DontAffect },
            { "(?)", DependencyModifier.Hidden }
        };

        private static readonly Dictionary<string, VersionOperator> DependencyOperators = new Dictionary<string, VersionOperator>()
        {
            { "<", VersionOperator.Less },
            { "<=", VersionOperator.LessOrEqual },
            { "=", VersionOperator.Equal },
            { ">=", VersionOperator.MoreOrEqual },
            { ">", VersionOperator.More }
        };

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

                DependencyInfo dependency = new DependencyInfo()
                {
                    ModId = match.Groups["modId"].Value,
                    Prefix = DependencyModifier.Required
                };

                if (match.Groups["prefix"].Success && DependencyPrefix.TryGetValue(match.Groups["prefix"].Value, out DependencyModifier prefix))
                    dependency.Prefix = prefix;

                if (match.Groups["operator"].Success && match.Groups["version"].Success)
                {
                    if (DependencyOperators.TryGetValue(match.Groups["operator"].Value, out VersionOperator versionOperator))
                        dependency.Operator = versionOperator;

                    if (Version.TryParse(match.Groups["version"].Value, out Version? version))
                        dependency.Version = version;
                }

                dependencies.Add(dependency);
            }

            return dependencies.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, DependencyInfo[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        [GeneratedRegex(@"(?:(?'prefix'[?!~]|\(\?\)) )?(?'modId'\w[\w- ]+\w+?)( (?'operator'<|<=|=|>=|>) (?'version'\S+))?")]
        private static partial Regex DependencyParserRegex();
    }
}
