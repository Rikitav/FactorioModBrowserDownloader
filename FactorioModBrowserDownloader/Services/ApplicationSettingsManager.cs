using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Converters;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.Services
{
    public static partial class ApplicationSettingsManager
    {
        private static readonly object IoSyncObj = new object();
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = false,
            WriteIndented = true
        };

        private static string ConfigFilePath
        {
            get
            {
                string nearAppCfgPath = Path.Combine(Environment.CurrentDirectory, "config.json");
                if (Directory.Exists(nearAppCfgPath))
                    return nearAppCfgPath;

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio-nexus", "config.json");
            }
        }

        public static SettingsContainer Current
        {
            get;
            private set;
        }

        static ApplicationSettingsManager()
        {
            Current = new SettingsContainer();

            //File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(Current));
            
            LoadSettings();
        }

        public static void LoadSettings()
        {
            lock (IoSyncObj)
            {
                using FileStream configStream = File.OpenRead(ConfigFilePath);
                SettingsContainer? container = JsonSerializer.Deserialize<SettingsContainer>(configStream);
                if (container == null)
                {
                    container = new SettingsContainer();
                    Debug.WriteLine("Settings container deserialization returned NULL instance! Default values assigned");
                }

                ValidateDeserializedContainer(container);
                Current = container;
            }
        }

        private static void ValidateDeserializedContainer(SettingsContainer container)
        {
            if (string.IsNullOrEmpty(container.GamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' setting cannot be null or empty");

            if (!Directory.Exists(container.GamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' contains invalid directory path (" + container.GamedataDirectory + ")");
        }
    }

    public class SettingsContainer : ViewModelBase
    {
        private static readonly string _gamedataDirectory_Default = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
        private static readonly bool _downloadOptionalDependencies_Default = false;
        private static readonly bool _dontFetchFullModsList_Default = false;

        private string? _gamedataDirectory = null;
        private bool? _downloadOptionalDependencies = null;
        private bool? _dontFetchFullModsList = null;

        [JsonPropertyName(nameof(GamedataDirectory)), JsonConverter(typeof(JsonFileSystemPathConverter))]
        public string GamedataDirectory
        {
            get => _gamedataDirectory ?? _gamedataDirectory_Default;
            set => Set(ref _gamedataDirectory, value);
        }

        [JsonPropertyName(nameof(DownloadOptionalDependencies))]
        public bool DownloadOptionalDependencies
        {
            get => _downloadOptionalDependencies ?? _downloadOptionalDependencies_Default;
            set => Set(ref _downloadOptionalDependencies, value);
        }

        [JsonPropertyName(nameof(DontFetchFullModsList))]
        public bool DontFetchFullModsList
        {
            get => _dontFetchFullModsList ?? _dontFetchFullModsList_Default;
            set => Set(ref _dontFetchFullModsList, value);
        }

        /*
        public bool HasValue(string propertyValue) => null != (propertyValue switch
        {
            nameof(GamedataDirectory) => _gamedataDirectory,
            _ => null
        });
        */
    }
}
