using FactorioModBrowserDownloader.Exetnsions;
using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal.Types
{
    public class ModPageEntryInfo : ViewModelBase
    {
        /// <summary>
        /// Returns <see cref="LatestRelease"/> or first release in <see cref="Releases"/> if <see cref="LatestRelease"/> is null
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ReleaseInfo? DisplayRelease => LatestRelease ?? Releases?.ElementAt(0);

        /// <summary>
        /// The latest version of the mod available for download. Absent when the namelist parameter is used.
        /// </summary>
        [JsonPropertyName("latest_release"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ReleaseInfo? LatestRelease { get; set; }

        /// <summary>
        /// Number of downloads.
        /// </summary>
        [JsonPropertyName("downloads_count"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int DownloadsCount { get; set; }

        /// <summary>
        /// The mod's machine-readable ID string.
        /// </summary>
        [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? ModId { get; set; }

        /// <summary>
        /// The Factorio username of the mod's author.
        /// </summary>
        [JsonPropertyName("owner"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? OwnerName { get; set; }

        /// <summary>
        /// A list of different versions of the mod available for download. Only when using namelist parameter.
        /// </summary>
        [JsonPropertyName("releases"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ReleaseInfo[]? Releases { get; set; }

        /// <summary>
        /// A shorter mod description.
        /// </summary>
        [JsonPropertyName("summary"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Summary { get; set; }

        /// <summary>
        /// The mod's human-readable name.
        /// </summary>
        [JsonPropertyName("title"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Title { get; set; }

        /// <summary>
        /// A single category describing the mod.
        /// </summary>
        [JsonPropertyName("category"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Category { get; set; }

        /// <summary>
        /// The score of the mod. *Only when not 0.
        /// </summary>
        [JsonPropertyName("score"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal Score { get; set; }
    }
}
