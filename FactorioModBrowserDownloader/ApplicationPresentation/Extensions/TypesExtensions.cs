using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                int bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
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
    }
}
