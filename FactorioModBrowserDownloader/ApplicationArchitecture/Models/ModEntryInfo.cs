using FactorioNexus.PresentationFramework;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class ModEntryInfo
    {
        /// <summary>
        /// Returns <see cref="LatestRelease"/> or first release in <see cref="Releases"/> if <see cref="LatestRelease"/> is null
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ReleaseInfo DisplayLatestRelease => LatestRelease ?? Releases?.LastOrDefault() ?? throw new MissingMemberException("\"" + Id + "\" doesn't have any releases");

        /// <summary>
        /// The latest version of the mod available for download. Absent when the namelist parameter is used.
        /// </summary>
        [JsonPropertyName("latest_release")]
        public ReleaseInfo? LatestRelease { get; set; }

        /// <summary>
        /// Number of downloads.
        /// </summary>
        [JsonPropertyName("downloads_count")]
        public int DownloadsCount { get; set; }

        /// <summary>
        /// The mod's machine-readable ID string.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Id { get; set; }

        /// <summary>
        /// The Factorio username of the mod's author.
        /// </summary>
        [JsonPropertyName("owner")]
        public string? OwnerName { get; set; }

        /// <summary>
        /// A list of different versions of the mod available for download. Only when using namelist parameter.
        /// </summary>
        [JsonPropertyName("releases")]
        public ReleaseInfo[]? Releases { get; set; }

        /// <summary>
        /// A shorter mod description.
        /// </summary>
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        /// <summary>
        /// The mod's human-readable name.
        /// </summary>
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        /// <summary>
        /// A single category describing the mod.
        /// </summary>
        [JsonPropertyName("category")]
        public CategoryInfo? Category { get; set; }

        /// <summary>
        /// The score of the mod. *Only when not 0.
        /// </summary>
        [JsonPropertyName("score")]
        public decimal? Score { get; set; }

        public bool TryFindRelease(DependencyVersionRange dependency, [NotNullWhen(true)] out ReleaseInfo? release)
        {
            ReleaseInfo[]? searchSpan = Releases ?? [LatestRelease ?? throw new MissingMemberException("\"" + Id + "\" doesn't have any releases")];
            release = searchSpan.LastOrDefault(dependency.IsInside);
            return release != null;
        }
    }
}
