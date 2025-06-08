using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Types
{
    public class PaginationInfo
    {
        /// <summary>
        /// Total number of mods that match your specified filters. 
        /// </summary>
        [JsonPropertyName("count"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int Count { get; set; }

        /// <summary>
        /// Utility links to mod portal api requests, preserving all filters and search queries. 
        /// </summary>
        [JsonPropertyName("page"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int PageIndex { get; set; }

        /// <summary>
        /// The current page number. 
        /// </summary>
        [JsonPropertyName("page_count"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int PageCount { get; set; }

        /// <summary>
        /// The total number of pages returned. 
        /// </summary>
        [JsonPropertyName("page_size"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public int PageSize { get; set; }

        /// <summary>
        /// The number of results per page. 
        /// </summary>
        [JsonPropertyName("links"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public LinksInfo? Links { get; set; }
    }
}
