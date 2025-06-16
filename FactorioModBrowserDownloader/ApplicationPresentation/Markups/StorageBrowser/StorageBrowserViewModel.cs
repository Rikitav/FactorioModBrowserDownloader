using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.Services;
using System.Collections.ObjectModel;

#pragma warning disable IDE0079
#pragma warning disable CA1822
namespace FactorioNexus.ApplicationPresentation.Markups.StorageBrowser;

public class StorageBrowserViewModel : ViewModelBase
{
    public ObservableCollection<ModStoreEntry> StoredMods
    {
        get => ModsStoringManager.StoredMods;
    }

    public StorageBrowserViewModel()
    {
        ModsStoringManager.ScanCurrentStorage();
    }
}
