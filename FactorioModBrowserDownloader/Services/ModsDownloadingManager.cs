using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FactorioNexus.Services
{
    public static class ModsDownloadingManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        public static readonly ObservableCollection<ModDownloadEntry> DownloadingModsList = [];
        
        public static ModDownloadEntry QueueModDownloading(ModPageFullInfo modPage, ReleaseInfo release, CancellationToken cancellationToken = default)
        {
            try
            {
                ModDownloadEntry? entry = FindEntry(modPage);
                if (entry == null)
                {
                    entry = new ModDownloadEntry(modPage, release);
                    QueueModDownloadingEntry(entry, cancellationToken);
                }

                return entry;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download mod {0}. {1}", [modPage.ModId, ex]);
                throw;
            }
            finally
            {
                DownloadingSemaphore.Release();
            }
        }

        private static async void QueueModDownloadingEntry(ModDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            DownloadingModsList.Add(entry);
            await DownloadingSemaphore.WaitAsync(cancellationToken);
            await entry.StartDownload();
            DownloadingModsList.Remove(entry);
        }

        public static ModDownloadEntry? FindEntry(ModPageFullInfo modPage)
        {
            return DownloadingModsList.FirstOrDefault(e => e.ModId == modPage.ModId);
        }
    }
}
