using System.IO;

namespace FactorioNexus.Utilities
{
    public static class StreamExtensions
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

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
        {
            long totalBytesRead = 0;
            byte[] buffer = new byte[bufferSize];

            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    progress?.Report(totalBytesRead);
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
