using FactorioNexus.ApplicationArchitecture.Serialization;
using FactorioNexus.PresentationFramework;
using System.IO;
using System.Text.Json.Serialization;

namespace FactorioNexus
{
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
    }
}
