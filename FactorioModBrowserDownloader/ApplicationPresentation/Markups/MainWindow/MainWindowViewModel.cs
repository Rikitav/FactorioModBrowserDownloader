using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object DownloadSyncObj = new object();

        private static readonly Dictionary<CheckBoolWrapper, CategoryInfo> _categorySelections = CategoryInfo.Known.Values.ToDictionary(_ => new CheckBoolWrapper());
        private static readonly Dictionary<CheckBoolWrapper, TagInfo> _tagSelections = TagInfo.Known.Values.ToDictionary(_ => new CheckBoolWrapper());
        private static readonly string[] _gameVersionSelections = ["0.13", "0.14", "0.15", "0.16", "0.17", "0.18", "1.0", "1.1", "2.0", "any"];

        // Mods display properties
        private ObservableCollection<ModPageFullInfo> _displayModsList = [];

        // Filter settings properties
        private string? _selectedGameVersion = null;
        private bool _includeDeprecatedMods = false;

        // Fallback display properties
        private bool _isCriticalError = false;
        private string? _criticalErrorMessage = null;

        // Debug display properties
        private string? _currentStatus = null;
        private bool _downloading = false;
        private string? _downloadingStatus = null;

        public Dictionary<CheckBoolWrapper, CategoryInfo> CategorySelections
        {
            get => _categorySelections;
        }

        public Dictionary<CheckBoolWrapper, TagInfo> TagSelections
        {
            get => _tagSelections;
        }

        public string[] GameVersionSelections
        {
            get => _gameVersionSelections;
        }

        public ObservableCollection<ModPageFullInfo> DisplayModsList
        {
            get => _displayModsList;
        }

        public string? SelectedGameVersion
        {
            get => _selectedGameVersion;
            set => Set(ref _selectedGameVersion, value);
        }

        public bool IncludeDeprecatedMods
        {
            get => _includeDeprecatedMods;
            set => Set(ref _includeDeprecatedMods, value);
        }

        public bool IsCriticalError
        {
            get => _isCriticalError;
            set => Set(ref _isCriticalError, value);
        }

        public string? CriticalErrorMessage
        {
            get => _criticalErrorMessage;
            set => Set(ref _criticalErrorMessage, value);
        }

        public string? CurrentState
        {
            get => _currentStatus;
            set => Set(ref _currentStatus, value);
        }

        public bool Downloading
        {
            get => _downloading;
            private set => Set(ref _downloading, value);
        }

        public string? DownloadingStatus
        {
            get => _downloadingStatus;
            private set => Set(ref _downloadingStatus, value);
        }

        public MainWindowViewModel()
        {
            KickStart();
        }

        private async void KickStart()
        {
            try
            {
                CurrentState = "Requesting entries";
                await ModsPresenterManager.StartNewBrowser(5);

                if (IsCriticalError)
                    return;

                CurrentState = "Got entries";
                await RequestModsList();
            }
            catch (OperationCanceledException)
            {
                IsCriticalError = true;
                CriticalErrorMessage = "Request cancelled";
            }
            catch (Exception ex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = ex.ToString();
            }
        }

        private async Task RequestModsList(CancellationToken cancellationToken = default)
        {
            // HIGHLIGHTED : https://mods.factorio.com/highlights
            // TRENDING: https://mods.factorio.com/browse/trending?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False
            // RECENTLY UPDATED : https://mods.factorio.com/browse/updated
            // MOST DOWNLOADED : https://mods.factorio.com/browse/downloaded?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False

            try
            {
                //FullModsList.Clear();
                Downloading = true;
                DownloadingStatus = "Requesting mods...";

                foreach (ModPageEntryInfo modEntry in ModsPresenterManager.LastBrowser.Results)
                {
                    try
                    {
                        CurrentState = "Requesting " + modEntry.ModId;
                        ModPageFullInfo modInfo = await ModsPresenterManager.FetchFullModInfo(modEntry);

                        if (FilterModPage(modInfo))
                            DisplayModsList.Add(modInfo);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("failed to download " + modEntry.ModId, ex);
                        continue;
                    }
                }

                Downloading = false;
                DownloadingStatus = null;
            }
            catch (TimeoutException tex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = "Requesting mods is timed out!";
            }
        }

        private bool FilterModPage(ModPageFullInfo modPage)
        {
            return true;
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs args)
        {

        }

        protected override void OnPropertyChanged(string propertyName)
        {

        }

        public class CheckBoolWrapper
        {
            public bool Value { get; set; } = false;
        }
    }
}
