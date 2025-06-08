using FactorioModBrowserDownloader.ModPortal.Requests;
using FactorioModBrowserDownloader.ModPortal.Types;
using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly FactorioModPortalClient client;

        private ObservableCollection<ModPageFullInfo>? _fullModsList = null;
        private Dictionary<CategoryInfo, bool> _categorySelections;

        public ObservableCollection<ModPageFullInfo>? FullModsList
        {
            get => _fullModsList;
            set => Set(ref _fullModsList, value);
        }

        public MainWindowViewModel()
        {
            _categorySelections = CategoryInfo.Known.Values.Select(category => new KeyValuePair<CategoryInfo, bool>(category, false)).ToDictionary();

            client = new FactorioModPortalClient();
            RequestModsList();
        }

        private async void RequestModsList()
        {
            // HIGHLIGHTED : https://mods.factorio.com/highlights
            // TRENDING: https://mods.factorio.com/browse/trending?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False
            // RECENTLY UPDATED : https://mods.factorio.com/browse/updated
            // MOST DOWNLOADED : https://mods.factorio.com/browse/downloaded?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False

            ModPortalList modsList = await client.SendRequest(new GetPortalModsListRequest()
            {
                SortProperty = "updated_at",
                SortOrder = "desc"
            });

            FullModsList = [];
            List<Task> tasks = [];
            foreach (ModPageEntryInfo modEntry in modsList.Results ?? throw new Exception())
            {
                if (modEntry.ModId == null)
                    continue;

                try
                {
                    ModPageFullInfo fullMod = await client.SendRequest(new GetFullModInfoRequest(modEntry.ModId));
                    FullModsList.Add(fullMod);
                    tasks.Add(client.DownloadThumbnail(fullMod));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to download " + modEntry.ModId, ex);
                }
            }

            await Task.WhenAll(tasks);
        }

        protected override void OnPropertyChanged(string propertyName)
        {

        }
    }
}
