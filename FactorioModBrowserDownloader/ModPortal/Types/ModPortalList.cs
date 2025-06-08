using FactorioNexus.ModPortal.Types;
using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal.Types
{
    public class ModPortalList
    {
        [JsonPropertyName("pagination"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("results"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ModPageEntryInfo[]? Results { get; set; }
    }
}
