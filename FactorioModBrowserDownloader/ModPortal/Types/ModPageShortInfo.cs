using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace FactorioModBrowserDownloader.ModPortal.Types
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
        /// Downloaded thumbnail bitmap object
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public BitmapSource? DownloadedThumbnail
        {
            get => _downloadedThumbnail;
            set => Set(ref _downloadedThumbnail, value);
        }
    }
}
