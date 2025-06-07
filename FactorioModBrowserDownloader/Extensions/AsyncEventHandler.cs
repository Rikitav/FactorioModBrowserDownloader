using FactorioModBrowserDownloader.ModPortal;

namespace FactorioModBrowserDownloader.Extensions
{
    public delegate ValueTask AsyncEventHandler<in TArgs>(FactorioModPortalClient client, TArgs args, CancellationToken cancellationToken = default(CancellationToken));
}
