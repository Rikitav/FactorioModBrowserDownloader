using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus
{
    public static class Constants
    {
        public const string ModsFactorioApiUrl = "https://mods.factorio.com/api";
        public const string AssetsFactorioUrl = "https://assets-mod.factorio.com";
        public const string PackagesFactorioUrl = "https://mods-storage.re146.dev";

        public static readonly string PrivateAppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio-nexus");
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    }
}
