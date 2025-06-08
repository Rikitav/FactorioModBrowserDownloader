using FactorioNexus.ModPortal;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public delegate ValueTask AsyncEventHandler<in TArgs>(FactorioModPortalClient client, TArgs args, CancellationToken cancellationToken = default(CancellationToken));
}
