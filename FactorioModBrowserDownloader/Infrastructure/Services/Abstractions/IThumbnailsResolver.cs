using FactorioNexus.Infrastructure.Models;
using System.Windows.Media.Imaging;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IThumbnailsResolver
    {
        public Task<BitmapSource> ResolveThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default);
    }
}
