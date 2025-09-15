using System.IO;

namespace FactorioNexus.Infrastructure.Models.Config
{
    public sealed partial class ApplicationSetings
    {
        private string _normalizedGamedataDirectory = string.Empty;
        private string _gamedataDirectory = "%AppData%\\Factorio";
        private int? _downloadOptionalDependencies = null;
        //private bool _dontFetchFullModsList = false;
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

        /*
        public bool DontFetchFullModsList
        {
            get => _dontFetchFullModsList;
            set => _dontFetchFullModsList = value;
        }
        */

        public TimeSpan DatabaseIrrelevantAfter
        {
            get => _databaseIrrelevantAfter;
            set => _databaseIrrelevantAfter = value;
        }

        /*
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
        */

        public void ValidateSettingsContainer()
        {
            if (string.IsNullOrEmpty(NormalizedGamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' setting cannot be null or empty");

            if (!Directory.Exists(NormalizedGamedataDirectory))
                throw new ApplicationException("\'GamedataDirectory\' contains invalid directory path (" + NormalizedGamedataDirectory + ")");
        }
    }
}
