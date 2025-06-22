using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus.Services
{
    public class ModStoreEntry : ViewModelBase
    {
        private DirectoryInfo _directory = null!;
        private ModInfo _info = null!;
        private BitmapSource? _thumbnail = null;
        private ModPageFullInfo? _modPage = null;

        public DirectoryInfo Directory
        {
            get => _directory;
            private set => Set(ref _directory, value);
        }

        public ModInfo Info
        {
            get => _info;
            private set => Set(ref _info, value);
        }

        public BitmapSource? Thumbnail
        {
            get => _thumbnail;
            private set => Set(ref _thumbnail, value);
        }

        public ModPageFullInfo ModPage
        {
            get => Get(ref _modPage, () => ModsBrowsingManager.FetchFullModInfo(Info.Name ?? throw new ArgumentException("Mod id cannot be null")).Result);
            private set => Set(ref _modPage, value);
        }

        public ModStoreEntry(DirectoryInfo modDirectory)
        {
            Directory = modDirectory;

            FileInfo modInfoFile = modDirectory.IndexFile("info.json");
            if (modInfoFile.Exists)
            {
                using FileStream infoStream = modInfoFile.OpenRead();
                Info = JsonSerializer.Deserialize<ModInfo>(infoStream, JsonClientAPI.Options) ?? throw new Exception("Failed to deserialize mod information");
            }
            else
            {
                throw new FileNotFoundException("Mods directory does not conatin info.json file!", modInfoFile.FullName);
            }

            FileInfo thumbnailFile = modDirectory.IndexFile("thumbnail.png");
            if (modInfoFile.Exists)
            {
                Thumbnail = ModsThumbnailsManager.LoadThumbnailFile(thumbnailFile);
            }
        }
    }
}
