using FactorioNexus.ApplicationArchitecture.Serialization;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.PresentationFramework;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus
{
    public class SettingsContainer : ViewModelBase
    {
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = false,
            WriteIndented = true
        };

        private static readonly string _gamedataDirectory_Default = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
        private static readonly bool _downloadOptionalDependencies_Default = false;
        private static readonly bool _dontFetchFullModsList_Default = false;
        private static readonly TimeSpan _databaseIrrelevantAfter_Default = TimeSpan.FromDays(1);

        private string? _gamedataDirectory = null;
        private bool? _downloadOptionalDependencies = null;
        private bool? _dontFetchFullModsList = null;
        private TimeSpan? _databaseIrrelevantAfter = null;

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

        public TimeSpan DatabaseIrrelevantAfter
        {
            get => _databaseIrrelevantAfter ?? _databaseIrrelevantAfter_Default;
            set => Set(ref _databaseIrrelevantAfter, value);
        }

        public static SettingsContainer LoadFromConfigFile()
        {
            SettingsContainer? container = null;
            string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");

            if (!File.Exists(configFilePath))
                configFilePath = Path.Combine(Constants.PrivateAppDataDirectory, "config.json");

            if (!File.Exists(configFilePath))
                return RecreteSettingsFile(configFilePath);

            try
            {
                using FileStream configStream = File.OpenRead(configFilePath);
                container = JsonSerializer.Deserialize<SettingsContainer>(configStream, serializerOptions);

                if (container == null)
                {
                    container = new SettingsContainer();
                    Debug.WriteLine("Settings container deserialization returned NULL instance! Default values assigned");
                }
            }
            catch
            {
                return RecreteSettingsFile(configFilePath);
            }

            ValidateSettingsContainer(container);
            return container;
        }

        private static SettingsContainer RecreteSettingsFile(string cfg)
        {
            SettingsContainer container = new SettingsContainer();
            string content = JsonSerializer.Serialize(container, serializerOptions);

            File.Delete(cfg);
            File.WriteAllText(cfg, content);
            return container;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateSettingsContainer(SettingsContainer container)
        {
            if (string.IsNullOrEmpty(container.GamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' setting cannot be null or empty");

            if (!Directory.Exists(container.GamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' contains invalid directory path (" + container.GamedataDirectory + ")");
        }
    }
}
