namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IViewModelLocator
    {
        IReadOnlyDictionary<Type, Type> ViewModelMap { get; }
    }
}
