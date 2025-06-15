using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Async assets
        private readonly object ExtendLock = new object();
        private CancellationTokenSource TokenSource = new CancellationTokenSource();
        private CancellationToken Cancell => TokenSource.Token;

        // Mods display properties
        private readonly ObservableCollection<ModPageFullInfo> _displayModsList = [];

        // Filter settings properties
        private readonly CheckboxValueWrapper<CategoryInfo>[] _categorySelections;
        private readonly CheckboxValueWrapper<TagInfo>[] _tagSelections;
        private readonly string[] _gameVersionSelections = ["0.13", "0.14", "0.15", "0.16", "0.17", "0.18", "1.0", "1.1", "2.0", "any"];
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

        // Commands
        private RelayCommand? _refreshModsListCommand = null;
        private RelayCommand? _downloadModCommand = null;

        public CheckboxValueWrapper<CategoryInfo>[] CategorySelections
        {
            get => _categorySelections;
        }

        public CheckboxValueWrapper<TagInfo>[] TagSelections
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

        public RelayCommand RefreshModsListCommand
        {
            get => _refreshModsListCommand ??= new RelayCommand(_ =>
            {
                RefreshList();
                ExtendList();
            });
        }

        public RelayCommand DownloadModCommand
        {
            get => _downloadModCommand ??= new RelayCommand(obj =>
            {
                if (obj is not ModPageFullInfo modPage)
                    throw new ArgumentException("Not a mod info instance");

                DownloadMod(modPage);
            });
        }

        public MainWindowViewModel()
        {
            // HIGHLIGHTED : https://mods.factorio.com/highlights
            // TRENDING: https://mods.factorio.com/browse/trending?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False
            // RECENTLY UPDATED : https://mods.factorio.com/browse/updated
            // MOST DOWNLOADED : https://mods.factorio.com/browse/downloaded?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False

            _categorySelections = CategoryInfo.Known.Values.Skip(1).ToCheckboxValues(RefreshModsListCommand);
            _tagSelections = TagInfo.Known.Values.ToCheckboxValues(RefreshModsListCommand);
            RefreshModsListCommand.Execute(null);
        }

        private async void DownloadMod(ModPageFullInfo modPage)
        {
            try
            {
                await ModsDownloadingManager.QueueModDownloading(modPage);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download mod " + modPage.ModId);
            }
        }

        private async void RefreshList()
        {
            try
            {
                CancellAndReset();
                Downloading = true;
                CurrentState = "Refreshing mods list";

                ModsPresenterManager.StartNewBrowser(25);
                await Task.Yield();
            }
            catch (OperationCanceledException)
            {
                //IsCriticalError = true;
                //CriticalErrorMessage = "Request cancelled";
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

        private async void ExtendList()
        {
            try
            {
                Downloading = true;
                CurrentState = "Requesting entries";
                await ModsPresenterManager.ExtendEntries(Cancell);

                if (IsCriticalError)
                    return;

                CurrentState = "Got entries";
                await RequestModsList();
            }
            catch (OperationCanceledException)
            {
                //IsCriticalError = true;
                //CriticalErrorMessage = "Request cancelled";
                return;
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

        private async Task RequestModsList()
        {
            try
            {
                Downloading = true;
                DownloadingStatus = "Requesting mods...";

                foreach (ModPageEntryInfo modEntry in ModsPresenterManager.LastResults)
                {
                    Cancell.ThrowIfCancellationRequested();

                    try
                    {
                        CurrentState = "Requesting " + modEntry.ModId;
                        ModPageFullInfo modInfo = await ModsPresenterManager.FetchFullModInfo(modEntry, Cancell);

                        if (FilterModPage(modInfo))
                            DisplayModsList.Add(modInfo);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("Timed out fetching mod {0}!", modEntry.ModId);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("failed to download " + modEntry.ModId, ex);
                        throw;
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
            if (TagSelections.Any(wrap => wrap.Checked))
            {
                if (modPage.Tags is null || !modPage.Tags.Any())
                    return false;

                if (!TagSelections.Select(wrap => wrap.Value).Union(modPage.Tags).Any())
                    return false;
            }

            if (CategorySelections.Any(wrap => wrap.Checked))
            {
                if (modPage.Category is null)
                    return false;

                if (modPage.Category.Name == "no-category")
                    return false;

                if (!CategorySelections.Select(wrap => wrap.Value).Contains(modPage.Category))
                    return false;
            }

            if (!IncludeDeprecatedMods)
            {
                if (modPage.Deprecated ?? false)
                    return false;
            }

            return true;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(RequireListExtending):
                    {
                        lock (ExtendLock)
                        {
                            if (RequireListExtending)
                            {
                                if (Downloading | IsCriticalError)
                                    return;

                                Debug.WriteLine("Extending mods list");
                                ExtendList();
                            }
                        }
                        break;
                    }

                case nameof(IncludeDeprecatedMods):
                case nameof(SelectedGameVersion):
                    {
                        RefreshModsListCommand.Execute(null);
                        break;
                    }
            }    
        }

        private void CancellAndReset()
        {
            lock (ExtendLock)
            {
                IsCriticalError = false;
                TokenSource.Cancel();
                TokenSource.Dispose();
                TokenSource = new CancellationTokenSource();
            }
        }
    }

    public class CheckboxValueWrapper<TValue>(TValue value, ICommand command)
    {
        public TValue Value { get; } = value;
        public ICommand Command { get; } = command;
        public bool Checked { get; set; } = false;
    }

    internal static class CollectionsExtensions
    {
        public static CheckboxValueWrapper<TValue>[] ToCheckboxValues<TValue>(this IEnumerable<TValue> source, ICommand command)
            => source.Select(value => new CheckboxValueWrapper<TValue>(value, command)).ToArray();
    }
}
