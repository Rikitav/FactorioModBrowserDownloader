using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationPresentation.Markups.StorageBrowser
{
    public class StorageBrowserViewModel : ViewModelBase
    {
        public ObservableCollection<ModPageFullInfo> StoredMods
        {
            get => ModsStoringManager.StoredMods;
        }
    }
}
