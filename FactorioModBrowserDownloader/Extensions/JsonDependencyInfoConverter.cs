using FactorioModBrowserDownloader.ModPortal.Types;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.Extensions
{
    /*
    public class JsonDependencyInfoConverter : JsonConverter<DependencyInfo>
    {
        private enum State
        {
            Modifier,
            ModId,
            Operator,
            Version,
            End
        }

        public override DependencyInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DependencyInfo dependencyInfo = new DependencyInfo();
            State state = State.Modifier;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string? value = reader.GetString();
                if (value == null)
                    break;

                if (string.IsNullOrEmpty(value))
                    continue;

                switch (state)
                {
                    case State.Modifier:
                        {
                            if (value == "!" | value == "?")
                                dependencyInfo.Modifier = value;

                            state = State.ModId;
                            continue;
                        }

                    case State.ModId:
                        {
                            dependencyInfo.ModId = value;
                            state = State.Operator;
                            continue;
                        }

                    case State.Operator:
                        {
                            dependencyInfo.Operator = value;
                            state = State.Version;
                            continue;
                        }

                    case State.Version:
                        {
                            dependencyInfo.Version = Version.Parse(value);
                            state = State.Version;
                            continue;
                        }

                    default:
                    case State.End:
                        {
                            break;
                        }
                }
            }

            return dependencyInfo;
        }

        public override void Write(Utf8JsonWriter writer, DependencyInfo value, JsonSerializerOptions options)
        {
            StringBuilder dependencyBuilder = new StringBuilder();
            if (value.Modifier != null)
            {
                dependencyBuilder.Append(value.Modifier);
                if (value.Modifier == "!")
                    dependencyBuilder.Append(' ');
            }

            if (value.ModId == null)
                throw new ArgumentNullException(nameof(value.ModId));

            dependencyBuilder.Append(value.ModId);
            dependencyBuilder.Append(' ');

            if (value.Operator != null && value.Version != null)
            {
                dependencyBuilder.Append(value.Operator);
                dependencyBuilder.Append(' ');
                dependencyBuilder.Append(value.Version.ToString());
            }
        }
    }
    */
}
