using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FactorioNexus.ModPortal.Types
{
    public partial class ReleaseInfo
    {
        /// <summary>
        /// Path to download for a mod. starts with "/download" and does not include a full url. See #Downloading Mods
        /// </summary>
        [JsonPropertyName("download_url"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// The file name of the release. Always seems to follow the pattern "{name}_{version}.zip"
        /// </summary>
        [JsonPropertyName("file_name"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? FileName { get; set; }

        /// <summary>
        /// A copy of the mod's info.json file, only contains factorio_version in short version, also contains an array of dependencies in full version
        /// </summary>
        [JsonPropertyName("info_json"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public required ModInfo ModInfo { get; set; }

        /// <summary>
        /// 8601) 	ISO 8601 for when the mod was released. (RFC 3339 nano)
        /// </summary>
        [JsonPropertyName("released_at"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public DateTime ReleasedDate { get; set; }

        /// <summary>
        /// The version string of this mod release. Used to determine dependencies.
        /// </summary>
        [JsonPropertyName("version"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public required Version Version { get; set; }

        /// <summary>
        /// The sha1 key for the file 
        /// </summary>
        [JsonPropertyName("sha1"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? ShaHash { get; set; }

        public DependencyInfo AsDependency()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException("Cannot create dependency from release without FileName");

            Match match = FileNameRegex().Match(FileName);
            return new DependencyInfo()
            {
                ModId = match.Groups["modId"].Value,
                Prefix = DependencyModifier.Required,
                Operator = VersionOperator.Equal,
                Version = Version
            };
        }

        [GeneratedRegex(@"(?'modId'\S+)_(?'version'\S+)\.zip")]
        private static partial Regex FileNameRegex();
    }
}
