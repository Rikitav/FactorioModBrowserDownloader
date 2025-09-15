using CommunityToolkit.Mvvm.ComponentModel;
using FactorioNexus.Infrastructure.Models;
using FactorioNexus.Infrastructure.Services;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public partial class ModStoreEntry : ObservableObject
    {
        private static readonly IFactorioNexusClient? _client;

        [ObservableProperty]
        private DirectoryInfo _directory = null!;

        [ObservableProperty]
        private ModInfo _info = null!;

        [ObservableProperty]
        private BitmapSource? _thumbnail = null;

        //[ObservableProperty]
        //private ModEntryFull? _modPage = null; .FetchFullModInfo(Info.Name)

        static ModStoreEntry()
        {
            if (App.Services != null)
            {
                _client = App.Services.GetRequiredService<IFactorioNexusClient>();
            }
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
            Info = JsonSerializer.Deserialize<ModInfo>(infoStream, FactorioNexusClient.JsonOptions) ?? throw new Exception("Failed to deserialize mod information");

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
