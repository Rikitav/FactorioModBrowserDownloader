using CommunityToolkit.Mvvm.ComponentModel;
using FactorioNexus.Data.Entities;
using FactorioNexus.Infrastructure.Models;
using System.ComponentModel;
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

    public partial class QueryFilterSettings : ObservableObject
    {
        private readonly ICommand _refreshCommand;

        [ObservableProperty]
        private OrderBy _selectedOrderBy = OrderBy.LastUpdated;

        [ObservableProperty]
        private string? _searchText = null;

        [ObservableProperty]
        private bool _useRegexSearch = false;

        [ObservableProperty]
        private string _selectedGameVersion = "2.0";

        [ObservableProperty]
        private bool _showDeprecated = false;

        [ObservableProperty]
        private bool _sortDescending = true;

        public CheckboxValueWrapper<CategoryInfo>[] CategorySelections { get; }
        public CheckboxValueWrapper<TagInfo>[] TagSelections { get; }
        public string[] GameVersionSelections { get; } = ["0.13", "0.14", "0.15", "0.16", "0.17", "0.18", "1.0", "1.1", "2.0", "any"];
        public OrderBy[] OrderBySelections { get; } = Enum.GetValues<OrderBy>();

        public QueryFilterSettings(ICommand refreshCommand)
        {
            _refreshCommand = refreshCommand;

            CategorySelections = CategoryInfo.Known.Values.Skip(1).ToCheckboxValues(refreshCommand);
            TagSelections = TagInfo.Known.Values.ToCheckboxValues(refreshCommand);
        }

        public bool CanPass(ModEntryEntity entity)
        {
            if (entity.DisplayLatestRelease == null)
                return false;

            if (!string.IsNullOrEmpty(SearchText))
            {
                if (!MatchesSearch(entity.Id, SearchText) && !MatchesSearch(entity.Title, SearchText))
                {
                    if (string.IsNullOrEmpty(entity.OwnerName) || !MatchesSearch(entity.OwnerName, SearchText))
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName != nameof(UseRegexSearch))
                _refreshCommand.Execute(null);
            */

            _refreshCommand.Execute(null);
        }

        private bool MatchesSearch(string input, string pattern)
        {
            if (UseRegexSearch)
                return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return RecursiveWildcardMatcher(input, 0, pattern, 0);
        }

        private static bool RecursiveWildcardMatcher(string text, int textPos, string pattern, int patternPos)
        {
            if (patternPos == pattern.Length)
                return textPos == text.Length;

            if (pattern[patternPos] == '*')
            {
                for (int skip = 0; textPos + skip <= text.Length; skip++)
                {
                    if (RecursiveWildcardMatcher(text, textPos + skip, pattern, patternPos + 1))
                        return true;
                }

                return false;
            }

            if (textPos < text.Length && (pattern[patternPos] == '?' || pattern[patternPos] == text[textPos]))
                return RecursiveWildcardMatcher(text, textPos + 1, pattern, patternPos + 1);

            return false;
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
