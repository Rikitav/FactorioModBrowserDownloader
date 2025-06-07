using FactorioModBrowserDownloader.ModPortal;

namespace FactorioModBrowserDownloader.Exetnsions
{
    public delegate ValueTask AsyncEventHandler<in TArgs>(FactorioModPortalClient client, TArgs args, CancellationToken cancellationToken = default(CancellationToken));
}
