using System.Text.Json.Serialization;
using System.Windows.Input;

namespace FactorioNexus.Infrastructure.Models
{
    public class ModEntryFull : ModEntryShort
    {
        /// <summary>
        /// A string describing the recent changes to a mod.
        /// </summary>
        [JsonPropertyName("changelog")]
        public string? Changelog { get; set; }

        /// <summary>
        /// for when the mod was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// for when the mod was last updated.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// for when the mod was last featured on the "Highlighted mods" tab.
        /// </summary>
        [JsonPropertyName("last_highlighted_at")]
        public DateTime HighlightedDate { get; set; }

        /// <summary>
        /// A longer description of the mod, in text only format.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// A URL to the mod's source code.
        /// </summary>
        [JsonPropertyName("source_url")]
        public string? SourceRepositoryUrl { get; set; }

        /// <summary>
        /// Deprecated: Use source_url instead. A link to the mod's github project page, just prepend "github.com/". Can be empty.
        /// </summary>
        [JsonPropertyName("github_path")]
        public string? SourceGithubUrl { get; set; }

        /// <summary>
        /// Usually a URL to the mod's main project page, but can be any string.
        /// </summary>
        [JsonPropertyName("homepage")]
        public string? Homepage { get; set; }

        /// <summary>
        /// A list of tag names that categorize the mod.
        /// </summary>
        [JsonPropertyName("tags")]
        public TagInfo[]? Tags { get; set; }

        /// <summary>
        /// The license that applies to the mod.
        /// </summary>
        [JsonPropertyName("license")]
        public LicenseInfo? Licenses { get; set; }

        /// <summary>
        /// True if the mod is marked as deprecated by its owner. Absent when false. 
        /// </summary>
        [JsonPropertyName("deprecated")]
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Command to download
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ICommand? DownloadCommand { get; set; }
    }
}
