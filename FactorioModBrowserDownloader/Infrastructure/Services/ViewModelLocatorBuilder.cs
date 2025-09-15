using FactorioNexus.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FactorioNexus.Infrastructure.Services
{
    public class ViewmodelLocatorBuilder(IServiceCollection services) : IViewModelLocatorBuilder
    {
        public IServiceCollection Services { get; } = services;
        public IDictionary<Type, Type> ModelsMap { get; } = new Dictionary<Type, Type>();
    }
}
