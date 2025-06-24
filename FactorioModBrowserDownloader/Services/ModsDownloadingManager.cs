using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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

        public static async Task<IEnumerable<DependencyVersionRange>> ScanRequiredDependencies(ReleaseInfo release)
        {
            if (release.ModInfo.Dependencies.Length == 0)
                return Enumerable.Empty<DependencyVersionRange>();

            Dictionary<string, DependencyVersionRange> dependencyInlineTree = [];
            await BuildInlineDependencyTree(release, dependencyInlineTree);

            List<DependencyVersionRange> matchedDependencies = [];
            foreach (DependencyVersionRange dependency in dependencyInlineTree.Values)
            {
                if (!await dependency.TryFindLatestMatchingRelease())
                    continue;

                if (ModsStoringManager.TryFindStore(dependency, out ModStoreEntry? _))
                    continue;

                matchedDependencies.Add(dependency);

                /*
                ModPageFullInfo dependencyModPage = await ModsBrowsingManager.FetchFullModInfo(dependency.ModId);
                if (!dependencyModPage.TryFindRelease(dependency, out ReleaseInfo? dependencyRelease))
                    continue;
 
                dependency.LatestMatchingRelease = dependencyRelease;
                matchedDependencies.Add(dependency);
                */
            } 

            return matchedDependencies;
        }

        public static async void QueueModDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            await QueuePackageDownloadingEntry(entry, cancellationToken);
        }

        /*
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
        */

        private static async Task BuildInlineDependencyTree(ReleaseInfo release, Dictionary<string, DependencyVersionRange> inlineTree)
        {
            foreach (DependencyInfo dependency in release.ModInfo.Dependencies)
            {
                if (dependency.ModId == "base" || dependency.ModId == "space-age")
                    continue;

                if (dependency.Prefix != DependencyModifier.Required)
                    continue;

                /*
                if (!ValidateRequiredDependency(dependency))
                    continue;
                */

                if (!inlineTree.TryGetValue(dependency.ModId, out DependencyVersionRange? range))
                {
                    range = new DependencyVersionRange(dependency);
                    inlineTree.Add(dependency.ModId, range);
                }
                else
                {
                    range.Tweak(dependency);
                }

                ModPageFullInfo dependencyModPage = await ModsBrowsingManager.FetchFullModInfo(dependency);
                if (!dependencyModPage.TryFindRelease(range, out ReleaseInfo? dependencyRelease))
                    dependencyRelease = dependencyModPage.DisplayLatestRelease;

                if (dependencyRelease.ModInfo.Dependencies.Length > 0)
                    await BuildInlineDependencyTree(dependencyRelease, inlineTree);
            }
        }

        public static async Task QueuePackageDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                DownloadingModsList.Add(entry);
                Debug.WriteLine("Added {0} entry to downloading queue", [entry.ModId]);

                await DownloadingSemaphore.WaitAsync(cancellationToken);
                Debug.WriteLine("{0} downloading entry started", [entry.ModId]);

                DirectoryInfo? modDir = await entry.StartDownload();
                if (modDir == null)
                {
                    Debug.WriteLine("Download entry \"{0}\" returned null directory. Considered failed to download", [entry.ModId]);
                    return;
                }

                ModsStoringManager.TryAddModStore(modDir);
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
