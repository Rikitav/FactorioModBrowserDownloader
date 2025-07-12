using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class ModEntryShort : ModEntryInfo
    {
        private BitmapSource? _downloadedThumbnail = null;

        /// <summary>
        /// The relative path to the thumbnail of the mod. For mods that have no thumbnail it may be absent or default
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }
    }
}
