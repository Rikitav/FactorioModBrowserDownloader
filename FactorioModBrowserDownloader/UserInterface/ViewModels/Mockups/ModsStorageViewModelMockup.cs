using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.UserInterface.ViewModels.Abstractions;
using FactorioNexus.UserInterface.Views.MainWindow;
using System.Collections.ObjectModel;

namespace FactorioNexus.UserInterface.ViewModels.Mockups
{
    public class ModsStorageViewModelMockup : ViewModelBase<ModsStorageView>, IModsStorageViewModel
    {
        public ObservableCollection<ModStoreEntry> StoredMods => [];
    }
}
