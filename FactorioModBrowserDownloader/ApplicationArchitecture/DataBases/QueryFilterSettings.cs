using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.PresentationFramework;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace FactorioNexus.ApplicationArchitecture.DataBases
{
    public enum OrderBy
    {
        Score,
        Downloads,
        LastUpdated
    }

    public class QueryFilterSettings : ViewModelBase
    {
        private readonly ICommand _refreshCommand;
        private readonly CheckboxValueWrapper<CategoryInfo>[] _categorySelections;
        private readonly CheckboxValueWrapper<TagInfo>[] _tagSelections;
        private readonly string[] _gameVersionSelections;
        private readonly OrderBy[] _orderBySelections;

        private string? _searchText = null;
        private bool _useRegexSearch = false;
        private string _selectedGameVersion = "2.0";
        private bool _showDeprecated = false;
        private OrderBy _selectedOrderBy = OrderBy.LastUpdated;
        private bool _sortDescending = true;

        public CheckboxValueWrapper<CategoryInfo>[] CategorySelections => _categorySelections;
        public CheckboxValueWrapper<TagInfo>[] TagSelections => _tagSelections;
        public string[] GameVersionSelections => _gameVersionSelections;
        public OrderBy[] OrderBySelections => _orderBySelections;

        public string? SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public bool UseRegexSearch
        {
            get => _useRegexSearch;
            set => Set(ref _useRegexSearch, value);
        }

        public string SelectedGameVersion
        {
            get => _selectedGameVersion;
            set => Set(ref _selectedGameVersion, value);
        }

        public bool ShowDeprecated
        {
            get => _showDeprecated;
            set => Set(ref _showDeprecated, value);
        }

        public OrderBy SelectedOrderBy
        {
            get => _selectedOrderBy;
            set => Set(ref _selectedOrderBy, value);
        }

        public bool SortDescending
        {
            get => _sortDescending;
            set => Set(ref _sortDescending, value);
        }

        public QueryFilterSettings(ICommand refreshCommand)
        {
            _refreshCommand = refreshCommand;
            _categorySelections = CategoryInfo.Known.Values.Skip(1).ToCheckboxValues(refreshCommand);
            _tagSelections = TagInfo.Known.Values.ToCheckboxValues(refreshCommand);
            _gameVersionSelections = ["0.13", "0.14", "0.15", "0.16", "0.17", "0.18", "1.0", "1.1", "2.0", "any"];
            _orderBySelections = Enum.GetValues<OrderBy>();
            ViewInitialized = true;
        }

        public bool CanPass(ModEntryEntity entity)
        {
            if (entity.DisplayLatestRelease == null)
                return false;

            if (!string.IsNullOrEmpty(SearchText))
            {
                Func<string, string, bool> SearchFunction = UseRegexSearch ? (input, pattern) => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : DefaultSearcher;
                if (!SearchFunction(entity.Id, SearchText) && !SearchFunction(entity.Title, SearchText))
                {
                    if (string.IsNullOrEmpty(entity.OwnerName))
                        return false;

                    if (!SearchFunction(entity.OwnerName, SearchText))
                        return false;
                }
            }

            IEnumerable<CheckboxValueWrapper<CategoryInfo>> selectedCategories = CategorySelections.Where(wrap => wrap.Checked);
            if (selectedCategories.Any())
            {
                if (entity.Category is null)
                    return false;

                if (entity.Category.Name == "no-category")
                    return false;

                if (!selectedCategories.Select(wrap => wrap.Value).Contains(entity.Category))
                    return false;
            }

            if (SelectedGameVersion != "any" && entity.DisplayLatestRelease.ModInfo.FactorioVersion?.ToString() != SelectedGameVersion)
            {
                return false;
            }

            return true;
        }

        public bool CanPass(ModEntryFull entry)
        {
            if ((entry.Deprecated ?? false) && !ShowDeprecated)
                return false;

            IEnumerable<CheckboxValueWrapper<TagInfo>> selectedTags = TagSelections.Where(wrap => wrap.Checked);
            if (selectedTags.Any())
            {
                if (entry.Tags is null || entry.Tags.Length == 0)
                    return false;

                if (selectedTags.Select(wrap => wrap.Value).Intersect(entry.Tags).Count() != selectedTags.Count())
                    return false;
            }

            return true;
        }

        public object? SortSelector(ModEntryEntity entity) => SelectedOrderBy switch
        {
            OrderBy.Score => entity.Score,
            OrderBy.Downloads => entity.DownloadsCount,
            OrderBy.LastUpdated => entity.DisplayLatestRelease?.ReleasedDate,
            _ => null,
        };

        public override void OnPropertyChanged(string propertyName)
        {
            if (propertyName != nameof(UseRegexSearch))
                _refreshCommand.Execute(null);
        }

        private static bool DefaultSearcher(string input, string pattern)
        {
            if (!input.Contains(pattern, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;

            //string specAsRegex = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            //return Regex.IsMatch(input, specAsRegex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }
    }

    public class CheckboxValueWrapper<TValue>(TValue value, ICommand command)
    {
        public TValue Value { get; } = value;
        public ICommand Command { get; } = command;
        public bool Checked { get; set; } = false;
    }

    public static class CollectionsExtensions
    {
        public static CheckboxValueWrapper<TValue>[] ToCheckboxValues<TValue>(this IEnumerable<TValue> source, ICommand command)
            => source.Select(value => new CheckboxValueWrapper<TValue>(value, command)).ToArray();
    }
}
