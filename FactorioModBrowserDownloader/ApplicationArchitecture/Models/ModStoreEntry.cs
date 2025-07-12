using FactorioNexus.PresentationFramework;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class ModStoreEntry : ViewModelBase
    {
        private DirectoryInfo _directory = null!;
        private ModInfo _info = null!;
        private BitmapSource? _thumbnail = null;
        private ModEntryFull? _modPage = null;

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

        public ModEntryFull? ModPage
        {
            get => GetAsync(ref _modPage, async () => await App.Instance.Client.FetchFullModInfo(Info.Name));
            private set => Set(ref _modPage, value);
        }

        public ModStoreEntry(DirectoryInfo modDirectory)
        {
            Directory = modDirectory;
            ReadInfoJson();
            ReadThumbnail();
        }

        [MemberNotNull(nameof(Info))]
        private void ReadInfoJson()
        {
            FileInfo modInfoFile = Directory.IndexFile("info.json");
            if (!modInfoFile.Exists)
                throw new FileNotFoundException("Mods directory does not contain info.json file!", modInfoFile.FullName);

            using FileStream infoStream = modInfoFile.OpenRead();
            Info = JsonSerializer.Deserialize<ModInfo>(infoStream, Constants.JsonOptions) ?? throw new Exception("Failed to deserialize mod information");

            if (string.IsNullOrEmpty(Info.Name))
                throw new InvalidOperationException("Mod name of \"" + Directory.Name + "\" store cannot be null or empty");
        }

        private void ReadThumbnail()
        {
            FileInfo thumbnailFile = Directory.IndexFile("thumbnail.png");
            if (thumbnailFile.Exists)
                Thumbnail = thumbnailFile.LoadThumbnailFile();
        }
    }
}
