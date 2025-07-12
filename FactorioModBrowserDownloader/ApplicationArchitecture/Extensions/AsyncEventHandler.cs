using FactorioNexus.ApplicationArchitecture.Services;

namespace FactorioNexus.ApplicationArchitecture.Extensions
{
    public delegate ValueTask AsyncEventHandler<in TArgs>(FactorioNexusClient client, TArgs args, CancellationToken cancellationToken = default(CancellationToken));
}
