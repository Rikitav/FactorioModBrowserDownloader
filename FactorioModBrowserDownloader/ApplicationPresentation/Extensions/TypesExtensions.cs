using System.IO;
using System.Text;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public static class TypesExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, Action<long>? progress = null, CancellationToken cancellationToken = default)
        {
            long totalBytesRead = 0;
            byte[] buffer = new byte[bufferSize];

            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    progress?.Invoke(totalBytesRead);
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;
                progress?.Invoke(totalBytesRead);
            }
        }

        public static string FirstLetterToUpper(this string source)
        {
            int index = -1;
            char target = source.FirstOrDefault(ch =>
            {
                index++;
                return char.IsLetter(ch);
            });

            if (target == default)
                return source;

            if (char.IsUpper(target))
                return source;

            StringBuilder builder = new StringBuilder(source);
            builder[index] = char.ToUpper(target);
            return builder.ToString();
        }
    }
}
