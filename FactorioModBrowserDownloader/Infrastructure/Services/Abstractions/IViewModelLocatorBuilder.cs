using Microsoft.Extensions.DependencyInjection;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IViewModelLocatorBuilder
    {
        IServiceCollection Services { get; }
        IDictionary<Type, Type> ModelsMap { get; }
    }
}
