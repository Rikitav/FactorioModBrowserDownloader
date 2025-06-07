using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal.Types
{
    public class ModPageFullInfo : ModPageShortInfo
    {
        /// <summary>
        /// A string describing the recent changes to a mod.
        /// </summary>
        [JsonPropertyName("changelog"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Changelog { get; set; }

        /// <summary>
        /// for when the mod was created.
        /// </summary>
        [JsonPropertyName("created_at"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// for when the mod was last updated.
        /// </summary>
        [JsonPropertyName("updated_at"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// for when the mod was last featured on the "Highlighted mods" tab.
        /// </summary>
        [JsonPropertyName("last_highlighted_at"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public DateTime HighlightedDate { get; set; }

        /// <summary>
        /// A longer description of the mod, in text only format.
        /// </summary>
        [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Description { get; set; }

        /// <summary>
        /// A URL to the mod's source code.
        /// </summary>
        [JsonPropertyName("source_url"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? SourceRepositoryUrl { get; set; }

        /// <summary>
        /// Deprecated: Use source_url instead. A link to the mod's github project page, just prepend "github.com/". Can be empty.
        /// </summary>
        [JsonPropertyName("github_path"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? SourceGithubUrl { get; set; }

        /// <summary>
        /// Usually a URL to the mod's main project page, but can be any string.
        /// </summary>
        [JsonPropertyName("homepage"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Homepage { get; set; }

        /// <summary>
        /// A list of tag names that categorize the mod.
        /// </summary>
        [JsonPropertyName("tags"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string[]? Tags { get; set; }

        /// <summary>
        /// The license that applies to the mod.
        /// </summary>
        [JsonPropertyName("license"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public LicenseInfo? Licenses { get; set; }

        /// <summary>
        /// True if the mod is marked as deprecated by its owner. Absent when false. 
        /// </summary>
        [JsonPropertyName("deprecated"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Deprecated { get; set; }
    }
}
