using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.Infrastructure.Models.Config
{
    public sealed partial class ApplicationSetings
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public static readonly string AppDataLocation = Path.Combine(Constants.PrivateAppDataDirectory, "config.json");

        private string _normalizedGamedataDirectory = Environment.ExpandEnvironmentVariables("%AppData%\\Factorio");
        private string _gamedataDirectory = "%AppData%\\Factorio";
        private int? _downloadOptionalDependencies = null;
        private TimeSpan _databaseIrrelevantAfter = TimeSpan.FromDays(1);

        public string GamedataDirectory
        {
            get => _gamedataDirectory;
            set
            {
                _gamedataDirectory = value;
                _normalizedGamedataDirectory = Environment.ExpandEnvironmentVariables(value);
            }
        }

        public string NormalizedGamedataDirectory
        {
            get => _normalizedGamedataDirectory;
        }

        public int? DownloadOptionalDependencies
        {
            get => _downloadOptionalDependencies;
            set => _downloadOptionalDependencies = value;
        }

        public TimeSpan DatabaseIrrelevantAfter
        {
            get => _databaseIrrelevantAfter;
            set => _databaseIrrelevantAfter = value;
        }

        public void ValidateSettingsContainer()
        {
            if (string.IsNullOrEmpty(NormalizedGamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' setting cannot be null or empty");

            if (!Directory.Exists(NormalizedGamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' contains invalid directory path (" + NormalizedGamedataDirectory + ")");
        }

        public static void RecreteSettingsFile()
        {
            File.Delete(AppDataLocation);
            using Stream defaultCfgStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FactorioNexus.config.json")!;
            using FileStream cfgFileStream = File.OpenWrite(AppDataLocation);
            defaultCfgStream.CopyTo(cfgFileStream);
        }
    }
}
