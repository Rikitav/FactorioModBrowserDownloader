using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.PresentationFramework;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationInterface.Dependencies
{
    public interface IModsStorageViewModel : IViewModel
    {
        public ObservableCollection<ModStoreEntry> StoredMods { get; }
    }
}
