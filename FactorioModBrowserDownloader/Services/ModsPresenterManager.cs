using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Requests;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;

namespace FactorioNexus.Services
{
    public enum SortBy
    {
        LastUpdates,
        LastCreateed,
        Name
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public static class ModsPresenterManager
    {
        private static bool Downloading = false;
        private static ModPortalList? LastList = null;
        private static GetPortalModsListRequest LastListRequest = null!;

        private static readonly List<ModPageEntryInfo> _modEntries = [];
        private static readonly Dictionary<string, ModPageFullInfo> _cachedMods = [];

        public static List<ModPageEntryInfo> Entries
        {
            get => _modEntries;
        }

        public static Dictionary<string, ModPageFullInfo> Cached
        {
            get => _cachedMods;
        }
        
        public static ModPageEntryInfo[] LastResults
        {
            get => LastList?.Results ?? [];
        }

        public static void StartNewBrowser(int? pageSize, SortBy? sortBy = null, SortOrder? sortOrder = null)
        {
            Entries.Clear();
            LastListRequest = new GetPortalModsListRequest()
            {
                PageIndex = 0,
                SortProperty = sortBy.ToProperty(),
                SortOrder = sortOrder.ToProperty(),
                PageSize = pageSize?.ToString() ?? "max",
                HideDeprecated = true
            };
        }

        public static async Task ExtendEntries(CancellationToken cancellationToken = default)
        {
            if (Downloading)
                return;

            try
            {
                Downloading = true;
                LastListRequest.PageIndex++;

                Debug.WriteLine("Extending mod entries. Current page : {0}", LastListRequest.PageIndex);
                LastList = await FactorioNexusClient.Instance.SendRequest(LastListRequest, cancellationToken);

                if (LastList.Results is null)
                {
                    Debug.WriteLine("Extending failed. Returned null entries.");
                    throw new Exception("Extending failed. Returned null entries.");
                }

                Debug.WriteLine("Extending successfull. GFathered {0} mods", LastList.Results.Length);
                _modEntries.AddRange(LastList.Results);
            }
            catch (OperationCanceledException)
            {
                LastListRequest.PageIndex--;
                LastList = null;

                Debug.WriteLine("Extending cancelled");
            }
            catch (Exception ex)
            {
                LastListRequest.PageIndex--;
                LastList = null;

                Debug.WriteLine("Failed to extend mods entries. {0}", ex);
                throw;
            }
            finally
            {
                Downloading = false;
            }
        }

        public static async Task<ModPageFullInfo> FetchFullModInfo(ModPageEntryInfo modEntry, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modEntry.ModId))
            {
                Debug.WriteLine("ModId is empty");
                throw new ArgumentException("ModId cannot be empty", nameof(modEntry.ModId));
            }

            if (Cached.TryGetValue(modEntry.ModId, out ModPageFullInfo? fullMod))
            {
                Debug.WriteLine("ModPageFullInfo was restored from cached mods");
                return fullMod;
            }

            try
            {
                Debug.WriteLine("Requesting {0} modId", modEntry.ModId);
                fullMod = await FactorioNexusClient.Instance.SendRequest(new GetFullModInfoRequest(modEntry.ModId), cancellationToken);

                Debug.WriteLine("ModId {0} cached", modEntry.ModId);
                _cachedMods.TryAdd(modEntry.ModId, fullMod);
                return fullMod;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("failed to cache {0}. {1}", modEntry.ModId, ex);
                throw;
            }
        }

        private static string? ToProperty(this SortOrder? sortOrder) => sortOrder switch
        {
            SortOrder.Ascending => "asc",
            SortOrder.Descending => "desc",
            _ => null
        };

        private static string? ToProperty(this SortBy? sortBy) => sortBy switch
        {
            SortBy.LastUpdates => "updated_at",
            SortBy.LastCreateed => "created_at",
            SortBy.Name => "name",
            _ => null
        };
    }
}
