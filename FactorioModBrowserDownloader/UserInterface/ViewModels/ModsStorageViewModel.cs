using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.UserInterface.ViewModels.Abstractions;
using FactorioNexus.UserInterface.Views.MainWindow;
using System.Collections.ObjectModel;

namespace FactorioNexus.UserInterface.ViewModels
{
    public class ModsStorageViewModel : ViewModelBase<ModsStorageView>, IModsStorageViewModel
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
