using System.IO;

namespace FactorioNexus
{
    public static class Constants
    {
        public const string ModsFactorioApiUrl = "https://mods.factorio.com/api";
        public const string AssetsFactorioUrl = "https://assets-mod.factorio.com";
        public const string PackagesFactorioUrl = "https://mods-storage.re146.dev";

        public static readonly string PrivateAppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio-nexus");
    }
}
