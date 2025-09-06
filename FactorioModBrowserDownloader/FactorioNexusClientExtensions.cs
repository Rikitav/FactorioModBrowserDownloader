using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationArchitecture.Requests;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus
{
    public static class FactorioNexusClientExtensions
    {
        private static readonly Dictionary<string, ModEntryFull> _cachedFullMods = [];

        public static async Task<BitmapSource> DownloadThumbnail(this IFactorioNexusClient client, ModEntryShort modPage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new ArgumentException("Cannot download thumbnail for mod page without thumbnail", nameof(modPage));

            try
            {
                string thumbnailUrl = Constants.AssetsFactorioUrl + modPage.Thumbnail;
                Debug.WriteLine("🖼️ Requesting thumbnail : {0}", [thumbnailUrl]);

                using (Stream contentStream = await client.SendDataRequest(thumbnailUrl, cancellationToken))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream();
                    contentStream.CopyTo(bitmapImage.StreamSource);
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download the thumbnail for \"{0}\" mod. {1}", [modPage.Id, ex]);
                throw;
            }
        }

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, ModEntryInfo modPage, ReleaseInfo releaseInfo, CancellationToken cancellationToken = default)
            => await client.DownloadPackage(modPage.Id, releaseInfo.Version, cancellationToken);

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, DependencyVersionRange dependency, CancellationToken cancellationToken = default)
        {
            if (dependency.LatestMatchingRelease == null)
                throw new ArgumentNullException(nameof(dependency), "Latest matching release wasn't found for this dependency");

            Version version = dependency.LatestMatchingRelease.Version;
            return await client.DownloadPackage(dependency.ModId, version, cancellationToken);
        }

        public static async Task<Stream> DownloadPackage(this IFactorioNexusClient client, string modId, Version version, CancellationToken cancellationToken = default)
        {
            try
            {
                string packageUri = Constants.PackagesFactorioUrl + string.Format("/{0}/{1}.zip", modId, version);
                Debug.WriteLine("📦 Requesting package : {0}", [packageUri]);
                return await client.SendDataRequest(packageUri, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download release package \"{0}\" for \"{1}\" mod. {2}", [version, modId, ex]);
                throw;
            }
        }

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, string modId, CancellationToken cancellationToken = default)
        {
            if (_cachedFullMods.TryGetValue(modId, out ModEntryFull? fullMod))
            {
                Debug.WriteLine("ModPageFullInfo {0} was restored from cached mods", [modId]);
                return fullMod;
            }

            try
            {
                Debug.WriteLine("Requesting \"{0}\"'s full mod page", [modId]);
                fullMod = await client.SendManagedRequest(new GetFullModInfoRequest(modId), cancellationToken);

                Debug.WriteLine("Id {0} cached", [modId]);
                _cachedFullMods.TryAdd(modId, fullMod);
                return fullMod;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("failed to cache {0}. {1}", [modId, ex]);
                throw;
            }
        }

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, ModEntryInfo modEntry, CancellationToken cancellationToken = default)
            => await client.FetchFullModInfo(modEntry.Id, cancellationToken);

        public static async Task<ModEntryFull> FetchFullModInfo(this IFactorioNexusClient client, DependencyInfo dependency, CancellationToken cancellationToken = default)
            => await client.FetchFullModInfo(dependency.ModId, cancellationToken);

        public static async Task<JsonDocument> GetModsDatabase(this IFactorioNexusClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponseMessage responce = await client.SendMessageRequest(new GetPortalModsListRequest(), cancellationToken);
            return JsonDocument.Parse(await responce.Content.ReadAsStreamAsync(cancellationToken));
        }
    }
}
