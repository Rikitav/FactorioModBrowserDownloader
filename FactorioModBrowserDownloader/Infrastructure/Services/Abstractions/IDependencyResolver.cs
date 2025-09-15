using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.Infrastructure.Extensions;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IDependencyResolver
    {
        public Task<IEnumerable<DependencyVersionRange>> ResolveRequiredDependencies(ReleaseInfo release);
    }
}
