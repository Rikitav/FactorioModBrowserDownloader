using FactorioNexus.ApplicationArchitecture.Models;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IDependencyResolver
    {
        public Task<IEnumerable<DependencyVersionRange>> ResolveRequiredDependencies(ReleaseInfo release);
    }
}
