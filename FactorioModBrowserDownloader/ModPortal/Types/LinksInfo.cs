using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal.Types
{
    public class LinksInfo
    {
        /// <summary>
        /// URL to the first page of the results, or null if you're already on the first page. 
        /// </summary>
        [JsonPropertyName("first"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? First { get; set; }

        /// <summary>
        /// URL to the previous page of the results, or null if you're already on the first page. 
        /// </summary>
        [JsonPropertyName("next"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Next { get; set; }

        /// <summary>
        /// URL to the next page of the results, or null if you're already on the last page. 
        /// </summary>
        [JsonPropertyName("prev"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Previous { get; set; }

        /// <summary>
        /// URL to the last page of the results, or null if you're already on the last page. 
        /// </summary>
        [JsonPropertyName("last"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Last { get; set; }
    }
}
