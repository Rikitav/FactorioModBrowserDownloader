using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using System.Diagnostics;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class DependencyResolver(IFactorioNexusClient client, IStoringManager storing) : DisposableBase<DependencyResolver>, IDependencyResolver
    {
        private static readonly string[] SkippingModsNames = ["base", "space-age", "quality"];
        private readonly IFactorioNexusClient Client = client;
        private readonly IStoringManager Storing = storing;

        public async Task<IEnumerable<DependencyVersionRange>> ResolveRequiredDependencies(ReleaseInfo release)
        {
            if (release.ModInfo.Dependencies ==null || release.ModInfo.Dependencies.Length == 0)
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

                if (Storing.TryFind(dependency.ModId, out ModStoreEntry? _))
                    continue;

                matchedDependencies.Add(dependency);
            }

            return matchedDependencies;
        }

        private async Task BuildInlineDependencyTree(ReleaseInfo release, Dictionary<string, DependencyVersionRange> inlineTree, int optionalDependencyResolveLevel)
        {
            if (release.ModInfo.Dependencies == null || release.ModInfo.Dependencies.Length == 0)
                return;

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
                    ModEntryFull dependencyModPage = await Client.FetchFullModInfo(dependency);
                    if (!dependencyModPage.TryFindRelease(range, out ReleaseInfo? dependencyRelease))
                        continue; //dependencyRelease = dependencyModPage.DisplayLatestRelease;

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
            if (dependency.Modifier.IsRequired)
                return true;

            if (App.Instance.Settings.DownloadOptionalDependencies && dependency.Modifier.IsOptional)
                return optionalDependencyResolveLevel > 0;

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
        }
    }
}
