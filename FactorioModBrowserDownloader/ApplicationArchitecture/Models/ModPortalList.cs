using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class ModPortalList
    {
        /*
        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }
        */

        [JsonPropertyName("results")]
        public required ModEntryInfo[] Results { get; set; }
    }

    /*
    public class PaginationInfo
    {
        /// <summary>
        /// Total number of mods that match your specified filters. 
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// Utility links to mod portal api requests, preserving all filters and search queries. 
        /// </summary>
        [JsonPropertyName("page")]
        public int PageIndex { get; set; }

        /// <summary>
        /// The current page number. 
        /// </summary>
        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        /// <summary>
        /// The total number of pages returned. 
        /// </summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        /// <summary>
        /// The number of results per page. 
        /// </summary>
        [JsonPropertyName("links")]
        public LinksInfo? Links { get; set; }
    }

    public class LinksInfo
    {
        /// <summary>
        /// URL to the first page of the results, or null if you're already on the first page. 
        /// </summary>
        [JsonPropertyName("first")]
        public string? First { get; set; }

        /// <summary>
        /// URL to the previous page of the results, or null if you're already on the first page. 
        /// </summary>
        [JsonPropertyName("next")]
        public string? Next { get; set; }

        /// <summary>
        /// URL to the next page of the results, or null if you're already on the last page. 
        /// </summary>
        [JsonPropertyName("prev")]
        public string? Previous { get; set; }

        /// <summary>
        /// URL to the last page of the results, or null if you're already on the last page. 
        /// </summary>
        [JsonPropertyName("last")]
        public string? Last { get; set; }
    }
    */
}
