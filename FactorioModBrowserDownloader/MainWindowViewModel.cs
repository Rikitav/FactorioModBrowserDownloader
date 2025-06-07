using FactorioModBrowserDownloader.Extensions;
using FactorioModBrowserDownloader.ModPortal;
using FactorioModBrowserDownloader.ModPortal.Requests;
using FactorioModBrowserDownloader.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace FactorioModBrowserDownloader
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly FactorioModPortalClient client;

        private ObservableCollection<ModPageFullInfo>? _fullModsList = null;
        private BitmapSource? _lastThumbnailDownload = null;

        public ObservableCollection<ModPageFullInfo>? FullModsList
        {
            get => _fullModsList;
            set => Set(ref _fullModsList, value);
        }

        public BitmapSource? LastThumbnailDownload
        {
            get => _lastThumbnailDownload;
            set => Set(ref _lastThumbnailDownload, value);
        }

        public MainWindowViewModel()
        {
            client = new FactorioModPortalClient();
            RequestModsList();
        }

        private async void RequestModsList()
        {
            ModPortalList modsList = await client.SendRequest(new GetPortalModsListRequest());
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

                    tasks.Add(client.DownloadThumbnail(fullMod).ContinueWith(thumb =>
                    {
                        LastThumbnailDownload = thumb.Result;
                        Debug.WriteLine("Thumbnail downloaded");
                    }));
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
