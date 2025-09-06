using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsStorageViewModel : ViewModelBase, IModsStorageViewModel
    {
        private readonly IStoringManager _storingManager;

        public ObservableCollection<ModStoreEntry> StoredMods
        {
            get => _storingManager.StoredMods;
        }

        public ModsStorageViewModel(IStoringManager storingManager)
        {
            _storingManager = storingManager;
        }
    }
}
