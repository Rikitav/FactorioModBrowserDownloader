using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;

namespace FactorioNexus.Services
{
    public static class ModsDownloadingManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);
        private static readonly string[] SkippingModsNames = [ "base", "space-age", "quality" ];

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
            await BuildInlineDependencyTree(release, dependencyInlineTree, 1);
            Debug.WriteLine("Inline dependency tree for mod \"{0}\" : [{1}]", release.FileName, string.Join(", ", dependencyInlineTree.Values));

            List<DependencyVersionRange> matchedDependencies = [];
            foreach (DependencyVersionRange dependency in dependencyInlineTree.Values)
            {
                if (!await dependency.TryFindLatestMatchingRelease())
                {
                    Debug.WriteLine("Failed to find dependency " + dependency.ToString());
                    continue;
                }

                if (ModsStoringManager.TryFindStore(dependency, out ModStoreEntry? _))
                    continue;

                matchedDependencies.Add(dependency);
            } 

            return matchedDependencies;
        }

        public static async void QueueModDownloadingEntry(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            await QueuePackageDownloadingEntry(entry, cancellationToken);
        }

        private static async Task BuildInlineDependencyTree(ReleaseInfo release, Dictionary<string, DependencyVersionRange> inlineTree, int optionalDependencyResolveLevel)
        {
            int myResolveLevel = optionalDependencyResolveLevel;
            foreach (DependencyInfo dependency in release.ModInfo.Dependencies)
            {
                if (SkippingModsNames.Contains(dependency.ModId))
                    continue;

                if (!IsDependencyRequired(dependency, myResolveLevel))
                    continue;

                if (!inlineTree.TryGetValue(dependency.ModId, out DependencyVersionRange? range))
                {
                    range = new DependencyVersionRange(dependency);
                    inlineTree.Add(dependency.ModId, range);
                }
                else
                {
                    if (range.TweakHistory.Count(dep => dep.ModId == dependency.ModId) > 5)
                        continue; // Recursion danger !

                    range.Tweak(dependency);
                }

                try
                {
                    ModPageFullInfo dependencyModPage = await ModsBrowsingManager.FetchFullModInfo(dependency);
                    if (!dependencyModPage.TryFindRelease(range, out ReleaseInfo? dependencyRelease))
                        continue; //dependencyRelease = dependencyModPage.DisplayLatestRelease;

                    if (dependencyRelease.ModInfo.Dependencies.Length > 0)
                        await BuildInlineDependencyTree(dependencyRelease, inlineTree, optionalDependencyResolveLevel - 1);
                }
                catch (RequestException rex)
                {
                    Debug.WriteLine("Failed to fetch mod \"{0}\" during dependencies resolutions. {1}", [dependency.ToString(), rex]);
                    continue;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to resolve mod \"{0}\". {1}", [dependency.ToString(), ex]);
                    continue;
                }
            }
        }

        private static bool IsDependencyRequired(DependencyInfo dependency, int optionalDependencyResolveLevel)
        {
            if (dependency.Prefix == DependencyModifier.Required)
                return true;

            if (ApplicationSettingsManager.Current.DownloadOptionalDependencies && dependency.Prefix == DependencyModifier.Optional)
                return optionalDependencyResolveLevel > 0;

            return false;
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
