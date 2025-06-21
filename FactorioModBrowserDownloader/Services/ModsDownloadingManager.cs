using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FactorioNexus.Services
{
    public static class ModsDownloadingManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        public static readonly ObservableCollection<PackageDownloadEntry> DownloadingModsList = [];
        public static readonly Dictionary<DependencyInfo, Task> DependencyDownloadingList = [];
        
        public static PackageDownloadEntry QueueModDownloading(ModPageFullInfo modPage, ReleaseInfo release, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!TryFindEntry(modPage, out PackageDownloadEntry? entry))
                {
                    entry = new ModDownloadEntry(modPage, release);
                    QueueModDownloadingEntry(entry, cancellationToken).ConfigureAwait(false);
                }

                return entry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to queue {0} mod download. {1}", [modPage.ModId, ex]);
                throw;
            }
        }

        public static async Task<PackageDownloadEntry> QueueDependencyDownloading(DependencyInfo dependency, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!TryFindEntry(dependency, out PackageDownloadEntry? entry))
                {
                    entry = new DependencyDownloadEntry(dependency);
                    await QueueModDownloadingEntry(entry, cancellationToken);
                }

                return entry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to queue {0} dependency download. {1}", [dependency.ModId, ex]);
                throw;
            }
        }

        public static IEnumerable<DependencyInfo> ScanRequiredDependencies(ReleaseInfo release)
        {
            if (release.ModInfo.Dependencies is null || release.ModInfo.Dependencies.Length == 0)
                yield break;

            foreach (DependencyInfo dependency in release.ModInfo.Dependencies)
            {
                if (dependency.ModId == "base" || dependency.ModId == "space-age")
                    continue;

                if (ModsStoringManager.TryFindStore(dependency, out _))
                    continue;

                if (dependency.Prefix != DependencyModifier.Required)
                    continue;

                yield return dependency;
            }
        }

        private static async Task QueueModDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                DownloadingModsList.Add(entry);
                Debug.WriteLine("Added {0} entry to downloading queue", [entry.ModId]);

                await DownloadingSemaphore.WaitAsync(cancellationToken);
                Debug.WriteLine("{0} downloading entry started", [entry.ModId]);

                await entry.StartDownload();
                Debug.WriteLine("{0} entry successfully downloaded", [entry.ModId]);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Cancelled {0} downloading entry", [entry.ModId]);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download {0} entry. {1}", [entry.ModId, ex]);
                throw;
            }
            finally
            {
                DownloadingSemaphore.Release();
                DownloadingModsList.Remove(entry);
                Debug.WriteLine("Removed {0} entry from downloading queue", [entry.ModId]);
            }
        }

        public static bool TryFindEntry(ModPageFullInfo modPage, [NotNullWhen(true)] out PackageDownloadEntry? result)
        {
            result = DownloadingModsList.FirstOrDefault(e => e.ModId == modPage.ModId);
            return result != null;
        }

        public static bool TryFindEntry(DependencyInfo dependency, [NotNullWhen(true)] out PackageDownloadEntry? result)
        {
            result = DownloadingModsList.FirstOrDefault(e => e.ModId == dependency.ModId);
            return result != null;
        }
    }
}
