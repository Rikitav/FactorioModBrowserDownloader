using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

#pragma warning disable IDE0079
#pragma warning disable CA1822
namespace FactorioNexus.ApplicationPresentation.Markups.ModsBrowser;

public class ModsBrowserViewModel : ViewModelBase
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
    private string? _nameSearchText = null;

    // Debug display properties
    private bool _isCriticalError = false;
    private string? _criticalErrorMessage = null;

    // Work display properties
    private bool _downloading = false;
    private string? _downloadingStatus = null;
    private bool _requireListExtending = false;

    // Commands
    private RelayCommand? _refreshModsListCommand = null;

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
        get => _requireListExtending;
        set => Set(ref _requireListExtending, value);
    }

    public string? NameSearchText
    {
        get => _nameSearchText;
        set => Set(ref _nameSearchText, value);
    }

    public RelayCommand RefreshModsListCommand
    {
        get => _refreshModsListCommand ??= new RelayCommand(_ =>
        {
            Debug.WriteLine("Refresh requested");
            RefreshList();
            ExtendList();
        });
    }

    public ModsBrowserViewModel()
    {
        // HIGHLIGHTED : https://mods.factorio.com/highlights
        // TRENDING: https://mods.factorio.com/browse/trending?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False
        // RECENTLY UPDATED : https://mods.factorio.com/browse/updated
        // MOST DOWNLOADED : https://mods.factorio.com/browse/downloaded?exclude_category=internal&factorio_version=2.0&show_deprecated=False&only_bookmarks=False

        _categorySelections = CategoryInfo.Known.Values.Skip(1).ToCheckboxValues(RefreshModsListCommand);
        _tagSelections = TagInfo.Known.Values.ToCheckboxValues(RefreshModsListCommand);

        //ModsBrowsingManager.StartNewBrowser(maxPage: true);
        RefreshList();
        ExtendList();
    }

    private void RefreshList()
    {
        try
        {
            Downloading = true;
            DownloadingStatus = "Refreshing mods list";

            CancellAndReset();
            DisplayModsList.Clear();
            ModsBrowsingManager.StartNewBrowser(25, SortBy.LastUpdates, SortOrder.Descending);
        }
        catch (OperationCanceledException)
        {
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

    private async void ExtendList()
    {
        bool acquiredLock = false;

        try
        {
            Monitor.Enter(ExtendLock, ref acquiredLock);

            {
                Downloading = true;
                DownloadingStatus = "Requesting entries";
                await ModsBrowsingManager.ExtendEntries(Cancell);

                await RequestFullMods();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            IsCriticalError = true;
            CriticalErrorMessage = ex.ToString();
        }
        finally
        {
            if (acquiredLock)
                Monitor.Exit(ExtendLock);

            Downloading = false;
            DownloadingStatus = null;

            /*
            if (RequireListExtending) // && ModsBrowsingManager.CanExtend()
                RaisePropertyChanged(nameof(RequireListExtending));
            */
        }
    }

    private async Task RequestFullMods()
    {
        try
        {
            DownloadingStatus = "Requesting mods...";
            foreach (ModPageEntryInfo modEntry in ModsBrowsingManager.LastResults)
            {
                Cancell.ThrowIfCancellationRequested();

                try
                {
                    //CurrentState = "Requesting " + modEntry.ModId;
                    ModPageFullInfo modInfo = await ModsBrowsingManager.FetchFullModInfo(modEntry, Cancell);

                    if (!FilterModPage(modInfo))
                        continue;

                    DisplayModsList.Add(modInfo);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("Timed out fetching mod {0}!", [modEntry.ModId]);
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to download {1}. {0}", [modEntry.ModId, ex]);
                    throw;
                }
            }
        }
        finally
        {
            DownloadingStatus = null;
        }
    }

    private bool FilterModPage(ModPageFullInfo modPage)
    {
        if (!IncludeDeprecatedMods)
        {
            if (modPage.Deprecated ?? false)
                return false;
        }

        IEnumerable<CheckboxValueWrapper<CategoryInfo>> selectedCategories = CategorySelections.Where(wrap => wrap.Checked);
        if (selectedCategories.Any())
        {
            if (modPage.Category is null)
                return false;

            if (modPage.Category.Name == "no-category")
                return false;

            if (!selectedCategories.Select(wrap => wrap.Value).Contains(modPage.Category, CategoryInfo.Comparer))
                return false;
        }

        IEnumerable<CheckboxValueWrapper<TagInfo>> selectedTags = TagSelections.Where(wrap => wrap.Checked);
        if (selectedTags.Any())
        {
            if (modPage.Tags is null || modPage.Tags.Length == 0)
                return false;

            if (selectedTags.Select(wrap => wrap.Value).Intersect(modPage.Tags, TagInfo.Comparer).Count() == selectedTags.Count())
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
                    if (!RequireListExtending)
                        return;

                    if (Monitor.IsEntered(ExtendLock))
                        return;

                    if (IsCriticalError)
                        return;

                    Debug.WriteLine("Extending mods list");
                    ExtendList();
                    break;
                }

            case nameof(IncludeDeprecatedMods):
                {
                    if (!ViewInitialized)
                        return;

                    //ModsBrowsingManager.ShowDeprecated = IncludeDeprecatedMods;
                    RefreshModsListCommand.Execute(null);
                    break;
                }

            case nameof(SelectedGameVersion):
                {
                    if (!ViewInitialized)
                        return;

                    if (string.IsNullOrEmpty(SelectedGameVersion))
                        return;

                    //ModsBrowsingManager.GameVersion = Version.Parse(SelectedGameVersion);
                    RefreshModsListCommand.Execute(null);
                    break;
                }

            case nameof(NameSearchText):
                {
                    if (!ViewInitialized)
                        return;

                    //ModsBrowsingManager.NameFilter = NameSearchText;
                    RefreshModsListCommand.Execute(null);
                    break;
                }
        }
    }

    private void CancellAndReset()
    {
        IsCriticalError = false;
        CriticalErrorMessage = null;

        TokenSource.Cancel();
        TokenSource.Dispose();
        TokenSource = new CancellationTokenSource();
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
