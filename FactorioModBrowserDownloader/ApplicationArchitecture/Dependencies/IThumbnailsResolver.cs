using FactorioNexus.ApplicationArchitecture.Models;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IThumbnailsResolver
    {
        public Task<BitmapSource> ResolveThumbnail(ModEntryShort modPage, CancellationToken cancellationToken = default);
    }
}
