namespace FactorioNexus.Utilities
{
    public static class TypesExtensions
    {
        public static T ThrowIfNull<T>(this T? obj, string message)
            => obj is null ? throw new NullReferenceException(message) : obj;

        public static bool Aggreagate<T>(this Exception exception)
        {
            if (exception is T)
                return true;

            if (exception.InnerException == null)
                return false;

            return Aggreagate<T>(exception.InnerException);
        }
    }
}
