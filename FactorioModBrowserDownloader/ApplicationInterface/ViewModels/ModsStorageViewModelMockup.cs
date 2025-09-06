using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsStorageViewModelMockup : ViewModelBase, IModsStorageViewModel
    {
        public ObservableCollection<ModStoreEntry> StoredMods => [];
    }
}
