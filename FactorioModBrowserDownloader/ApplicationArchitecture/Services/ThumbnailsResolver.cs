using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class ThumbnailsResolver(IFactorioNexusClient client) : DisposableBase<ThumbnailsResolver>, IThumbnailsResolver
    {
        private static readonly string NexusAppdataDirectory = Constants.PrivateAppDataDirectory;
        private const int MaxDownloading = 5;

        private readonly object SyncObj = new object();
        private readonly IFactorioNexusClient Client = client;

        private Dictionary<string, BitmapSource> MemoryCachedThumbnails = [];
        private SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

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
                return resolvedThumbnail ?? throw new FailedThumbnailException();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get thumbnail image for {0}. {1}", [modPage.Id, ex]);
                throw new FailedThumbnailException();
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
                    Debug.WriteLine("Thumbnail for {0} was restored from cached thumbnails", [modPage.Id]);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Debug.WriteLine("Failed to cache thumbnail image to file for {0}. {1}", [modPage.Id, ex]);
                return null;
            }
        }

        private async Task<BitmapSource?> TryDownloadThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default)
        {
            try
            {
                // Downloading thumbnail from Factorio assets server
                await DownloadingSemaphore.WaitAsync(cancellationToken);
                BitmapSource bitmap = await Client.DownloadThumbnail(modPage, cancellationToken);

                // Debug message
                Debug.WriteLine("Thumbnail for {0} was downloaded from assets server", [modPage.Id]);
                return bitmap;
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Debug.WriteLine("Failed to download thumbnail image for {0}. {1}", [modPage.Id, ex]);
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
                Debug.WriteLine("Failed to cache thumbnail image to memory for {0}. {1}", [modPage.Id, ex]);
            }
        }

        private static void SaveThumbnailCache(ModEntryShort modPage, FileInfo cachedFile, BitmapSource bitmap)
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
                Debug.WriteLine("Failed to cache thumbnail image to file for {0}. {1}", [modPage.Id, ex]);
            }
        }

        private static FileInfo GetCachedThumbnailFile(ModEntryShort modPage)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new NullReferenceException("Thumbnail is null!");

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

    public class MissingThumbnailException()
        : Exception()
    { }

    public class FailedThumbnailException()
        : Exception()
    { }
}
