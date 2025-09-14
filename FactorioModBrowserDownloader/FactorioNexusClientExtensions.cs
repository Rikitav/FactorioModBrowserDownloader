using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationArchitecture.Requests;
using FactorioNexus.ApplicationArchitecture.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace FactorioNexus
{
    public static class FactorioNexusClientExtensions
    {
        private static readonly Dictionary<string, ModEntryFull> _cachedFullMods = [];

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, ModEntryInfo modPage, ReleaseInfo releaseInfo, CancellationToken cancellationToken = default)
            => await client.DownloadPackage(modPage.Id, releaseInfo.Version, cancellationToken);

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, DependencyVersionRange dependency, CancellationToken cancellationToken = default)
        {
            ILogger<IFactorioNexusClient> logger = client is FactorioNexusClient fnxc
                ? fnxc.Logger : NullLogger<IFactorioNexusClient>.Instance;

            if (dependency.LatestMatchingRelease == null)
            {
                logger.LogError("Latest matching release for '{id}' wasn't found for this dependency", dependency.ModId);
                throw new ArgumentNullException(nameof(dependency), "Latest matching release wasn't found for this dependency");
            }

            Version version = dependency.LatestMatchingRelease.Version;
            return await client.DownloadPackage(dependency.ModId, version, cancellationToken);
        }

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, string modId, Version version, CancellationToken cancellationToken = default)
        {
            ILogger<IFactorioNexusClient> logger = client is FactorioNexusClient fnxc
                ? fnxc.Logger : NullLogger<IFactorioNexusClient>.Instance;

            try
            {
                string packageUri = Constants.PackagesFactorioUrl + string.Format("/{0}/{1}.zip", modId, version);
                using (HttpResponseMessage responce = await client.Request(packageUri, cancellationToken))
                    return await responce.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to download release package '{ver}' for '{id}' mod.", version, modId);
                throw;
            }
        }

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, string modId, CancellationToken cancellationToken = default)
        {
            ILogger<IFactorioNexusClient> logger = client is FactorioNexusClient fnxc
                ? fnxc.Logger : NullLogger<IFactorioNexusClient>.Instance;

            if (_cachedFullMods.TryGetValue(modId, out ModEntryFull? fullMod))
            {
                logger.LogTrace("ModPageFullInfo '{id}' was restored from cached mods", modId);
                return fullMod;
            }

            try
            {
                logger.LogTrace("Requesting '{id}'s full mod page", modId);
                fullMod = await client.RequestManaged(new GetFullModInfoRequest(modId), cancellationToken);

                _cachedFullMods.TryAdd(modId, fullMod);
                logger.LogTrace("Id '{id}' cached", modId);
                return fullMod;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to cache {id}.", modId);
                throw;
            }
        }

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, ModEntryInfo modEntry, CancellationToken cancellationToken = default)
            => await client.FetchFullModInfo(modEntry.Id, cancellationToken);

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, DependencyInfo dependency, CancellationToken cancellationToken = default)
            => await client.FetchFullModInfo(dependency.ModId, cancellationToken);

        public static async Task<JsonDocument> GetModsDatabase(this IFactorioNexusClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponseMessage responce = await client.Request(new GetPortalModsListRequest(), cancellationToken);
            return JsonDocument.Parse(await responce.Content.ReadAsStreamAsync(cancellationToken));
        }
    }
}
