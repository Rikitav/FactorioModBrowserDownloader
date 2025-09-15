using FactorioNexus.Infrastructure.Services.Abstractions;
using System.Collections.ObjectModel;

namespace FactorioNexus.Infrastructure.Services
{
    public class ViewModelLocator(IDictionary<Type, Type> viewModelMap) : IViewModelLocator
    {
        public IReadOnlyDictionary<Type, Type> ViewModelMap { get; } = new ReadOnlyDictionary<Type, Type>(viewModelMap);
    }
}
