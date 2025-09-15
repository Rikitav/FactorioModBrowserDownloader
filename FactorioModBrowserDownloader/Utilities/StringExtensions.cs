namespace FactorioNexus.Utilities
{
    public static class StringExtensions
    {
        public static string FirstLetterToUpper(this string source)
        {
            char[] chars = source.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char lookChar = chars[i];
                if (!char.IsLetter(lookChar))
                    continue;

                if (char.IsUpper(lookChar))
                    return source;

                chars[i] = char.ToUpper(lookChar);
                return new string(chars);
            }

            return source;
        }
    }
}
