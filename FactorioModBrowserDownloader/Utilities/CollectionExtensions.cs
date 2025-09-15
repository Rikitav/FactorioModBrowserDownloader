namespace FactorioNexus.Utilities
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> OrderBy<T, Key>(this IEnumerable<T> values, bool descending, Func<T, Key> keySelector)
            => descending ? values.OrderByDescending(keySelector) : values.OrderBy(keySelector);
    }
}
