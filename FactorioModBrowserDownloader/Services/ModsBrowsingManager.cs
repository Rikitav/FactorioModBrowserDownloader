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
        
        private static readonly ObservableCollection<ModPageEntryInfo> _fetchedModEntries = [];
        private static readonly Dictionary<string, ModPageFullInfo> _cachedFullMods = [];

        public static ObservableCollection<ModPageEntryInfo> Entries => _fetchedModEntries;

        public static Dictionary<string, ModPageFullInfo> CachedFullMods => _cachedFullMods;

        public static ModPageEntryInfo[] LastResults => LastList?.Results ?? [];

        /*
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

        public static Version? GameVersion
        {
            get;
            set;
        }
        */

        public static void StartNewBrowser(int? pageSize = null, SortBy? sortBy = null, SortOrder? sortOrder = null, bool maxPage = false)
        {
            Entries.Clear();
            LastList = null;

            _requestInstance = new GetPortalModsListRequest()
            {
                PageIndex = 0,
                SortProperty = sortBy.ToProperty(),
                SortOrder = sortOrder.ToProperty(),
                PageSize = pageSize?.ToString() ?? (maxPage ? "max" : null)
            };
        }

        public static bool CanExtend()
        {
            if (LastList?.Pagination == null)
                return false;

            LinksInfo? links = LastList.Pagination.Links;
            if (links == null)
                return false;

            if (links.First == null && links.Last == null)
                return false;

            return links.Next != null;
        }

        public static async Task ExtendEntries(CancellationToken cancellationToken = default)
        {
            if (_requestInstance.PageSize == "max")
            {
                await FullFecthEntries(cancellationToken);
            }
            else
            {
                await PageNextEntries(cancellationToken);
            }
        }

        private static async Task FullFecthEntries(CancellationToken cancellationToken = default)
        {
            if (Downloading)
                return;

            try
            {
                Downloading = true;
                Debug.WriteLine("Fetching full mods entries. Current page : MAX");
                LastList = await FactorioNexusClient.Instance.SendRequest(_requestInstance, cancellationToken);

                if (LastList.Results is null)
                {
                    Debug.WriteLine("Extending failed. Returned null entries.");
                    throw new Exception("Extending failed. Returned null entries.");
                }

                Debug.WriteLine("Extending successfull. Fetched {0} mods", [LastList.Results.Length]);
                Array.ForEach(LastResults, _fetchedModEntries.Add);
            }
            catch (OperationCanceledException)
            {
                _requestInstance.PageIndex--;
                LastList = null;

                Debug.WriteLine("Fetching canceled");
            }
            catch (Exception ex)
            {
                _requestInstance.PageIndex--;
                LastList = null;

                Debug.WriteLine("Failed to fetch mods entries. {0}", [ex]);
                throw;
            }
            finally
            {
                Downloading = false;
            }
        }

        private static async Task PageNextEntries(CancellationToken cancellationToken = default)
        {
            if (Downloading)
                return;

            if (LastList != null && !CanExtend())
                return;

            try
            {
                Downloading = true;
                _requestInstance.PageIndex++;

                Debug.WriteLine("Extending mod entries. Current page : {0}", [_requestInstance.PageIndex]);
                LastList = await FactorioNexusClient.Instance.SendRequest(_requestInstance, cancellationToken);

                if (LastList.Results is null)
                {
                    Debug.WriteLine("Extending failed. Returned null entries.");
                    throw new Exception("Extending failed. Returned null entries.");
                }

                Debug.WriteLine("Extending successfull. Fetched {0} mods", [LastList.Results.Length]);
                Array.ForEach(LastResults, _fetchedModEntries.Add);
            }
            catch (OperationCanceledException)
            {
                _requestInstance.PageIndex--;
                LastList = null;

                Debug.WriteLine("Extending canceled");
            }
            catch (Exception ex)
            {
                _requestInstance.PageIndex--;
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
            if (CachedFullMods.TryGetValue(modId, out ModPageFullInfo? fullMod))
            {
                Debug.WriteLine("ModPageFullInfo {0} was restored from cached mods", [modId]);
                return fullMod;
            }

            try
            {
                Debug.WriteLine("Requesting \"{0}\"'s full mod page", [modId]);
                fullMod = await FactorioNexusClient.Instance.SendRequest(new GetFullModInfoRequest(modId), cancellationToken);

                Debug.WriteLine("ModId {0} cached", [modId]);
                _cachedFullMods.TryAdd(modId, fullMod);
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
