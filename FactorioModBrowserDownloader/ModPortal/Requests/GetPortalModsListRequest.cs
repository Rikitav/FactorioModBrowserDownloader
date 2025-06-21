using FactorioNexus.ModPortal.Types;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Requests
{
    public class GetPortalModsListRequest() : ApiRequestBase<ModPortalList>("mods")
    {
        /// <summary>
        /// Only return non-deprecated mods.
        /// </summary>
        [JsonPropertyName("hide_deprecated")]
        public bool HideDeprecated { get; set; } = true;

        /// <summary>
        /// Page number you would like to show. Makes it so you can see a certain part of the list without getting detail on all
        /// </summary>
        [JsonPropertyName("page")]
        public int? PageIndex { get; set; }

        /// <summary>
        /// The amount of results to show in your search
        /// </summary>
        [JsonPropertyName("page_size")]
        public string? PageSize { get; set; }

        /// <summary>
        /// Sort results by this property. Defaults to name when not defined. Ignored for page_size=max queries.
        /// </summary>
        [JsonPropertyName("sort")]
        public string? SortProperty { get; set; }

        /// <summary>
        /// Sort results ascending or descending. Defaults to descending when not defined. Ignored for page_size=max queries.
        /// </summary>
        [JsonPropertyName("sort_order")]
        public string? SortOrder { get; set; }

        /// <summary>
        /// Return only mods that match the given names. Either comma-separated names or supply the namelist parameter more than once. Response will include releases instead of latest_release.
        /// </summary>
        [JsonPropertyName("namelist")]
        public string? Namelist { get; set; }

        /// <summary>
        /// Only return non-deprecated mods compatible with this Factorio version 
        /// </summary>
        [JsonPropertyName("version")]
        public Version? Version { get; set; }
    }
}
