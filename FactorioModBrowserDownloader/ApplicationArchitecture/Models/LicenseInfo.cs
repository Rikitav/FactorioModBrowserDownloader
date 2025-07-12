using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class LicenseInfo
    {
        /// <summary>
        /// A short description of the license.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The unique id of the license.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The internal name of the license.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The human-readable title of the license.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Usually a URL to the full license text, but can be any string. 
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
