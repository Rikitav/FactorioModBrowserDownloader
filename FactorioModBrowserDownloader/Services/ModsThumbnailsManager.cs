using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace FactorioNexus.Services
{
    public static class ModsThumbnailsManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        private static readonly object SyncObj = new object();
        private static readonly Dictionary<string, BitmapSource> MemoryCachedThumbnails = [];
        private static readonly string NexusAppdataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio-nexus");

        static ModsThumbnailsManager()
        {
            Directory.CreateDirectory(NexusAppdataDirectory);
            Directory.CreateDirectory(Path.Combine(NexusAppdataDirectory, "assets"));
        }

        public static async Task QueueThumbnailDownloading(ModPageShortInfo modPage, CancellationToken cancellationToken = default)
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
                modPage.DisplayThumbnail = memoryCachedThumbnail;
                return;
            }

            // Getting path for thumbnail cached in FileSystem
            string cachedThumbnailPath = GetCachedThumbnailPath(modPage);

            try
            {
                // Trying to restore cached thumbnail from file
                if (TryLoadCachedThumbnail(modPage, cachedThumbnailPath))
                    return;

                // Trying to download thumbnail
                if (await TryDownloadThumbnail(modPage, cancellationToken))
                    return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get thumbnail image for {0}. {1}", [modPage.ModId, ex]);
                throw new FailedThumbnailException();
            }
            finally
            {
                if (modPage.DisplayThumbnail != null)
                {
                    MemoryThumbnailCache(modPage, modPage.DisplayThumbnail);
                    SaveThumbnailCache(modPage, cachedThumbnailPath, modPage.DisplayThumbnail);
                }
            }
        }

        public static BitmapSource LoadThumbnailFile(FileInfo thumbnailFile)
        {
            using FileStream fileStream = thumbnailFile.OpenRead();
            BitmapImage openedImage = new BitmapImage();

            // Copying from filestream
            openedImage.BeginInit();
            openedImage.StreamSource = new MemoryStream();
            fileStream.CopyTo(openedImage.StreamSource);
            openedImage.EndInit();

            return openedImage;
        }

        private static bool TryLoadCachedThumbnail(ModPageShortInfo modPage, string cachePath)
        {
            try
            {
                lock (SyncObj)
                {
                    if (!IsThumbnailCached(cachePath))
                        return false;

                    // Setting as used display
                    modPage.DisplayThumbnail = LoadThumbnailFile(new FileInfo(cachePath));

                    // Debug message
                    Debug.WriteLine("Thumbnail for {0} was restored from cached thumbnails", [modPage.ModId]);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Debug.WriteLine("Failed to cache thumbnail image to file for {0}. {1}", [modPage.ModId, ex]);
                return false;
            }
        }

        private static async Task<bool> TryDownloadThumbnail(ModPageShortInfo modPage, CancellationToken cancellationToken = default)
        {
            try
            {
                // Downloading thumbnail from Factorio's assets server
                await DownloadingSemaphore.WaitAsync(cancellationToken);
                modPage.DisplayThumbnail = await FactorioNexusClient.Instance.DownloadThumbnail(modPage, cancellationToken);

                // Debug message
                Debug.WriteLine("Thumbnail for {0} was downloaded from assets server", [modPage.ModId]);
                return true;
            }
            catch (Exception ex)
            {
                // Something went wrong during thumbnail loading
                Debug.WriteLine("Failed to download thumbnail image for {0}. {1}", [modPage.ModId, ex]);
                return false;
            }
            finally
            {
                DownloadingSemaphore.Release();
            }
        }

        private static void SaveThumbnailCache(ModPageShortInfo modPage, string cachePath, BitmapSource bitmap)
        {
            try
            {
                if (IsThumbnailCached(cachePath))
                    return;

                // Encoding bitmap
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // Caching used thumbnail in file system
                using FileStream fileStream = File.Create(cachePath);
                encoder.Save(fileStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to cache thumbnail image to file for {0}. {1}", [modPage.ModId, ex]);
            }
        }

        private static void MemoryThumbnailCache(ModPageShortInfo modPage, BitmapSource bitmap)
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
                Debug.WriteLine("Failed to cache thumbnail image to memory for {0}. {1}", [modPage.ModId, ex]);
            }
        }

        private static bool IsThumbnailCached(string cachedThumbnailPath)
        {
            return File.Exists(cachedThumbnailPath);
        }

        private static string GetCachedThumbnailPath(ModPageShortInfo modPage)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                return string.Empty;

            return NexusAppdataDirectory + modPage.Thumbnail.Replace('/', '\\');
        }
    }

    public class MissingThumbnailException()
        : Exception()
    { }

    public class FailedThumbnailException()
        : Exception()
    { }
}
