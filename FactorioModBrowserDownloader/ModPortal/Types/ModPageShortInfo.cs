using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ModPortal.Types
{
    public class ModPageShortInfo : ModPageEntryInfo
    {
        private BitmapSource? _downloadedThumbnail = null;

        /// <summary>
        /// The relative path to the thumbnail of the mod. For mods that have no thumbnail it may be absent or default
        /// </summary>
        [JsonPropertyName("thumbnail"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Thumbnail { get; set; }

        /// <summary>
        /// Thumbnail bitmap object to display
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public BitmapSource? DisplayThumbnail
        {
            get => _downloadedThumbnail;
            set => Set(ref _downloadedThumbnail, value);
        }
    }
}
