using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.UserInterface.Views.MainWindow;
using System.Collections.ObjectModel;

namespace FactorioNexus.UserInterface.ViewModels.Abstractions
{
    public interface IModsStorageViewModel : IViewModel<ModsStorageView>
    {
        public ObservableCollection<ModStoreEntry> StoredMods { get; }
    }
}
