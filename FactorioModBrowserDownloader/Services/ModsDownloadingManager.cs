using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
                if (TryFindEntry(modPage, out PackageDownloadEntry? entry))
                    return entry;

                entry = new ModDownloadEntry(modPage, release);
                QueueModDownloadingEntry(entry, cancellationToken);
                return entry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to queue {0} mod download. {1}", [modPage.ModId, ex]);
                throw;
            }
        }

        public static async Task<IEnumerable<DependencyInfo>> ScanRequiredDependencies(ReleaseInfo release)
        {
            if (release.ModInfo.Dependencies.Length == 0)
                return Enumerable.Empty<DependencyInfo>();
            
            List<DependencyInfo> dependencyInlineTree = [];
            await BuildInlineDependencyTree(release, dependencyInlineTree);
            return dependencyInlineTree;
        }

        public static async void QueueModDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            await QueuePackageDownloadingEntry(entry, cancellationToken);
        }

        private static async Task BuildInlineDependencyTree(ReleaseInfo release, List<DependencyInfo> tree)
        {
            foreach (DependencyInfo dependency in release.ModInfo.Dependencies.Where(ValidateRequiredDependency))
            {
                if (!ValidateRequiredDependency(dependency))
                    continue;

                if (!FindDependency(tree, dependency))
                    continue;

                ModPageFullInfo dependencyModPage = await ModsBrowsingManager.FetchFullModInfo(dependency);
                ReleaseInfo dependencyRelease = dependencyModPage.FindRelease(dependency);

                if (dependencyRelease.ModInfo.Dependencies.Length > 0)
                    await BuildInlineDependencyTree(dependencyRelease, tree);
            }
        }

        private static bool FindDependency(List<DependencyInfo> tree, DependencyInfo toSearch)
        {
            DependencyInfo? found = tree.FirstOrDefault(current => current.ModId == toSearch.ModId);

            if (found == null)
            {
                tree.Add(toSearch);
                return true;
            }

            int versionCompare = toSearch.Version == null
                ? found.Version == null ? 0 : 1
                : found.Version == null ? -1 : toSearch.Version.CompareTo(found.Version);

            if (versionCompare == 1)
            {
                tree.Remove(found);
                tree.Add(toSearch);
                return true;
            }

            return false;
        }

        private static bool ValidateRequiredDependency(DependencyInfo dependency)
        {
            if (dependency.ModId == "base" || dependency.ModId == "space-age")
                return false;

            if (ModsStoringManager.TryFindStore(dependency, out _))
                return false;

            if (dependency.Prefix != DependencyModifier.Required)
                return false;

            return true;
        }

        public static async Task QueuePackageDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
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
