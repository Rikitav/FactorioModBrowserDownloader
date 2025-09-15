using FactorioNexus.Infrastructure.Services;

namespace FactorioNexus.Infrastructure.Extensions
{
    public delegate ValueTask AsyncEventHandler<in TArgs>(FactorioNexusClient client, TArgs args, CancellationToken cancellationToken = default(CancellationToken));
}
