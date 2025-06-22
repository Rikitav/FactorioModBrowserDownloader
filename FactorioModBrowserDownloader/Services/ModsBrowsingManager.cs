using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Requests;
using FactorioNexus.ModPortal.Types;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
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

    public static class ModsBrowsingManager
    {
        private static bool Downloading = false;
        private static ModPortalList? LastList = null;
        private static GetPortalModsListRequest _requestInstance = new GetPortalModsListRequest();
        
        private static readonly ObservableCollection<ModPageEntryInfo> _modEntries = [];
        private static readonly Dictionary<string, ModPageFullInfo> _cachedMods = [];

        /// <summary>
        /// Current list of mods
        /// </summary>
        public static ObservableCollection<ModPageEntryInfo> Entries => _modEntries;

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, ModPageFullInfo> Cached => _cachedMods;

        /// <summary>
        /// 
        /// </summary>
        public static GetPortalModsListRequest RequestInstance => _requestInstance;

        /// <summary>
        /// 
        /// </summary>
        public static ModPageEntryInfo[] LastResults => LastList?.Results ?? [];

        public static string? NameFilter
        {
            get;
            set;
        }

        public static bool ShowDeprecated
        {
            get;
            set;
        }

        public static void StartNewBrowser(int? pageSize, SortBy? sortBy = null, SortOrder? sortOrder = null)
        {
            Entries.Clear();
            LastList = null;

            _requestInstance = new GetPortalModsListRequest()
            {
                PageIndex = 0,
                SortProperty = sortBy.ToProperty(),
                SortOrder = sortOrder.ToProperty(),
                PageSize = pageSize?.ToString() ?? "max",
                Namelist = NameFilter,
                HideDeprecated = !ShowDeprecated
            };
        }

        public static async Task ExtendEntries(CancellationToken cancellationToken = default)
        {
            if (Downloading)
                return;

            try
            {
                Downloading = true;
                RequestInstance.PageIndex++;
                RequestInstance.Namelist = NameFilter;

                Debug.WriteLine("Extending mod entries. Current page : {0}", [RequestInstance.PageIndex]);
                LastList = await FactorioNexusClient.Instance.SendRequest(RequestInstance, cancellationToken);

                if (LastList.Results is null)
                {
                    Debug.WriteLine("Extending failed. Returned null entries.");
                    throw new Exception("Extending failed. Returned null entries.");
                }

                Debug.WriteLine("Extending successfull. Fetched {0} mods", [LastList.Results.Length]);
                Array.ForEach(LastResults, _modEntries.Add);
            }
            catch (OperationCanceledException)
            {
                RequestInstance.PageIndex--;
                LastList = null;

                Debug.WriteLine("Extending canceled");
            }
            catch (Exception ex)
            {
                RequestInstance.PageIndex--;
                LastList = null;

                Debug.WriteLine("Failed to extend mods entries. {0}", [ex]);
                throw;
            }
            finally
            {
                Downloading = false;
            }
        }

        public static async Task<ModPageFullInfo> FetchFullModInfo(ModPageEntryInfo modEntry, CancellationToken cancellationToken = default)
            => await FetchFullModInfo(modEntry.ModId, cancellationToken);

        public static async Task<ModPageFullInfo> FetchFullModInfo(DependencyInfo dependency, CancellationToken cancellationToken = default)
            => await FetchFullModInfo(dependency.ModId, cancellationToken);

        public static async Task<ModPageFullInfo> FetchFullModInfo(string modId, CancellationToken cancellationToken = default)
        {
            if (Cached.TryGetValue(modId, out ModPageFullInfo? fullMod))
            {
                Debug.WriteLine("ModPageFullInfo {0} was restored from cached mods", [modId]);
                return fullMod;
            }

            try
            {
                Debug.WriteLine("Requesting \"{0}\"'s full mod page", [modId]);
                fullMod = await FactorioNexusClient.Instance.SendRequest(new GetFullModInfoRequest(modId), cancellationToken);

                Debug.WriteLine("ModId {0} cached", [modId]);
                _cachedMods.TryAdd(modId, fullMod);
                return fullMod;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("failed to cache {0}. {1}", [modId, ex]);
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
