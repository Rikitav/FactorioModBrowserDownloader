using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Types
{
    public class ModPortalList
    {
        [JsonPropertyName("pagination"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("results"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public required ModPageEntryInfo[] Results { get; set; }
    }
}
