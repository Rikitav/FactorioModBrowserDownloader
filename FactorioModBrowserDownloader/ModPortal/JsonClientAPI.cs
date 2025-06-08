using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal
{
    public static class JsonClientAPI
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
