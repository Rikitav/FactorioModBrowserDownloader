using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
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
        private bool _wantExpanding = false;

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

        public Dictionary<string, ModPageFullInfo>.ValueCollection CachedMods
        {
            get => ModsPresenterManager.Cached.Values;
        }

        public List<ModPageEntryInfo> ModsEntries
        {
            get => ModsPresenterManager.Entries;
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
            private set => Set(ref _isCriticalError, value);
        }

        public string? CriticalErrorMessage
        {
            get => _criticalErrorMessage;
            private set => Set(ref _criticalErrorMessage, value);
        }

        public string? CurrentState
        {
            get => _currentStatus;
            private set => Set(ref _currentStatus, value);
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

        public bool RequireListExtending
        {
            get => _wantExpanding;
            set => Set(ref _wantExpanding, value);
        }

        public MainWindowViewModel()
        {
            // HIGHLIGHTED : https://mods.factorio.com/highlights
            // TRENDING: https://mods.factorio.com/browse/trending?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False
            // RECENTLY UPDATED : https://mods.factorio.com/browse/updated
            // MOST DOWNLOADED : https://mods.factorio.com/browse/downloaded?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False

            ModsPresenterManager.StartNewBrowser(25);
            ExtendList();
        }

        private async void ExtendList(CancellationToken cancellationToken = default)
        {
            try
            {
                Downloading = true;
                CurrentState = "Requesting entries";
                await ModsPresenterManager.ExtendEntries(cancellationToken);

                if (IsCriticalError)
                    return;

                CurrentState = "Got entries";
                await RequestModsList(cancellationToken);
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
            finally
            {
                Downloading = false;
                DownloadingStatus = null;
            }
        }

        private async Task RequestModsList(CancellationToken cancellationToken = default)
        {
            try
            {
                Downloading = true;
                DownloadingStatus = "Requesting mods...";

                foreach (ModPageEntryInfo modEntry in ModsPresenterManager.LastResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        CurrentState = "Requesting " + modEntry.ModId;
                        ModPageFullInfo modInfo = await ModsPresenterManager.FetchFullModInfo(modEntry, cancellationToken);

                        if (FilterModPage(modInfo))
                            DisplayModsList.Add(modInfo);
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("Timed out fetching mod {0}!", modEntry.ModId);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("failed to download " + modEntry.ModId, ex);
                        continue;
                    }
                }
            }
            finally
            {
                Downloading = false;
                DownloadingStatus = null;
            }
        }

        private bool FilterModPage(ModPageFullInfo modPage)
        {
            return true;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(RequireListExtending):
                    {
                        if (RequireListExtending && !Downloading)
                        {
                            Debug.WriteLine("Extending mods list");
                            ExtendList();
                        }

                        break;
                    }
            }    
        }

        public class CheckBoolWrapper
        {
            public bool Value { get; set; } = false;
        }
    }
}
