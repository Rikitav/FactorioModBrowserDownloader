using FactorioNexus.Infrastructure.Models;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace FactorioNexus.Infrastructure.Services
{
    public class ThumbnailsResolver : DisposableBase<ThumbnailsResolver>, IThumbnailsResolver
    {
        private static readonly string NexusAppdataDirectory = Constants.PrivateAppDataDirectory;
        private const int MaxDownloading = 5;

        private readonly object SyncObj = new object();
        private readonly IFactorioNexusClient Client;
        private readonly ILogger<ThumbnailsResolver> Logger;

        private Dictionary<string, BitmapSource> MemoryCachedThumbnails = [];
        private SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        public ThumbnailsResolver(ILogger<ThumbnailsResolver> logger, IFactorioNexusClient client)
        {
            Directory.CreateDirectory(Path.Combine(Constants.PrivateAppDataDirectory, "assets"));
            Client = client;
            Logger = logger;
        }

        public async Task<BitmapSource> ResolveThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default)
        {
            // Checking if mod page has an thumbnail
            if (string.IsNullOrEmpty(modPage.Thumbnail))
            {
                throw new MissingThumbnailException();
            }

            // Checking if thumbnail for this mod page was already been cached in memory
            if (MemoryCachedThumbnails.TryGetValue(modPage.Thumbnail, out BitmapSource? memoryCachedThumbnail))
            {
                // If so, setting it
                return memoryCachedThumbnail;
            }

            // Getting path for thumbnail cached in FileSystem
            FileInfo cachedThumbnailFile = GetCachedThumbnailFile(modPage);
            BitmapSource? resolvedThumbnail = null;

            try
            {
                // Trying to restore cached thumbnail from file
                resolvedThumbnail = TryLoadCachedThumbnail(modPage, cachedThumbnailFile);

                // Trying to download thumbnail
                resolvedThumbnail ??= await TryDownloadThumbnail(modPage, cancellationToken);

                // Returning resolved thumbnail
                return resolvedThumbnail ?? throw new ThumbnailFailedException();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get thumbnail image for '{id}'.", modPage.Id);
                throw new ThumbnailFailedException();
            }
            finally
            {
                if (resolvedThumbnail != null)
                {
                    MemoryThumbnailCache(modPage, resolvedThumbnail);
                    SaveThumbnailCache(modPage, cachedThumbnailFile, resolvedThumbnail);
                }
            }
        }

        private BitmapSource? TryLoadCachedThumbnail(ModEntryShort modPage, FileInfo cachedFile)
        {
            try
            {
                lock (SyncObj)
                {
                    if (!cachedFile.Exists)
                        return null;

                    // Setting as used display
                    BitmapSource bitmap = cachedFile.LoadThumbnailFile();

                    // Debug message
                    Logger.LogTrace("Thumbnail for '{id}' was restored from cached thumbnails.", modPage.Id);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Logger.LogError(ex, "Failed to cache thumbnail image to file for '{id}'.", modPage.Id);
                return null;
            }
        }

        private async Task<BitmapSource?> TryDownloadThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default)
        {
            try
            {
                // Downloading thumbnail from Factorio assets server
                await DownloadingSemaphore.WaitAsync(cancellationToken);
                BitmapSource bitmap = await DownloadThumbnail(modPage, cancellationToken);

                // Debug message
                Logger.LogTrace("Thumbnail for '{id}' was downloaded from assets server.", modPage.Id);
                return bitmap;
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Logger.LogError(ex, "Failed to download thumbnail image for '{id}'.", modPage.Id);
                return null;
            }
            finally
            {
                DownloadingSemaphore.Release();
            }
        }

        private void MemoryThumbnailCache(ModEntryShort modPage, BitmapSource bitmap)
        {
            try
            {
                if (string.IsNullOrEmpty(modPage.Thumbnail))
                    return;

                if (MemoryCachedThumbnails.ContainsKey(modPage.Thumbnail))
                    return;

                // Caching used thumbnail in memory
                MemoryCachedThumbnails.Add(modPage.Thumbnail, bitmap);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to cache thumbnail image to memory for '{id}'.", modPage.Id);
            }
        }

        private void SaveThumbnailCache(ModEntryShort modPage, FileInfo cachedFile, BitmapSource bitmap)
        {
            try
            {
                if (cachedFile.Exists)
                    return;

                // Encoding bitmap
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // Caching used thumbnail in file system
                using FileStream fileStream = cachedFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                encoder.Save(fileStream);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to cache thumbnail image to file for '{id}'.", modPage.Id);
            }
        }

        public async Task<BitmapSource> DownloadThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new ArgumentException("Cannot download thumbnail for mod page without thumbnail", nameof(modPage));

            try
            {
                string thumbnailUrl = Constants.AssetsFactorioUrl + modPage.Thumbnail;
                Logger.LogTrace("Requesting thumbnail for mod '{id}'.", thumbnailUrl);

                using (HttpResponseMessage responce = await Client.Request(thumbnailUrl, cancellationToken))
                {
                    using (Stream contentStream = await responce.Content.ReadAsStreamAsync(cancellationToken))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();

                        bitmapImage.StreamSource = new MemoryStream();
                        contentStream.CopyTo(bitmapImage.StreamSource);

                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        return bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download the thumbnail for '{id}'.", modPage.Id);
                throw;
            }
        }

        private static FileInfo GetCachedThumbnailFile(ModEntryShort modPage)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new NullReferenceException("Thumbnail path is null");

            return new FileInfo(NexusAppdataDirectory + modPage.Thumbnail.Replace('/', '\\'));
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (DownloadingSemaphore != null)
            {
                DownloadingSemaphore.Dispose();
                DownloadingSemaphore = null!;
            }

            if (MemoryCachedThumbnails != null)
            {
                MemoryCachedThumbnails.Clear();
                MemoryCachedThumbnails = null!;
            }
        }
    }

    public class MissingThumbnailException() : Exception();
    public class ThumbnailFailedException() : Exception();
}
