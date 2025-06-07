using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal.Types
{
    public class LicenseInfo
    {
        /// <summary>
        /// A short description of the license.
        /// </summary>
        [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Description { get; set; }

        /// <summary>
        /// The unique id of the license.
        /// </summary>
        [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Id { get; set; }

        /// <summary>
        /// The internal name of the license.
        /// </summary>
        [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Name { get; set; }

        /// <summary>
        /// The human-readable title of the license.
        /// </summary>
        [JsonPropertyName("title"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Title { get; set; }

        /// <summary>
        /// Usually a URL to the full license text, but can be any string. 
        /// </summary>
        [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Url { get; set; }
    }
}
